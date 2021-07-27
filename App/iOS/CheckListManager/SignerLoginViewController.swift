//
//  SignerLoginViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 10/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import UIKit

class SignerLoginViewController: UIViewController, UITextFieldDelegate {

    @IBOutlet var loginView: UIView!
    @IBOutlet var userCodeTextField: UITextField!
    @IBOutlet var passwordTextField: UITextField!
    @IBOutlet var closeButton: UIButton!
    
    let appDelegate = (UIApplication.shared as! MyApplication)
    let loader: LoadingViewController = LoadingViewController.shared
    

    override func viewDidLoad() {
        super.viewDidLoad()
        userCodeTextField.delegate = self
        passwordTextField.delegate = self
    }

    
    @IBAction func closeButtonAction(_ sender: UIButton) {
        self.dismiss(animated: true, completion: nil)
    }
    
    @IBAction func signButtonAction(_ sender: UIButton) {
        performSign()
    }
    
    func performSign() {
        
        guard let usercode = self.userCodeTextField.text, !usercode.isEmpty else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Insert a user code")
            return
        }
        
        guard let password = self.passwordTextField.text, !password.isEmpty else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Missing password")
            return
        }
        
        guard let company = UserDefaults.standard.string(forKey: "company") else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Missing company")
            return
        }
        
        do {
            try SignersManager.shared.checkSigner(code: usercode)
        } catch {
            self.appDelegate.displayMessage(Title: "Error", Message: error.localizedDescription)
            return
        }
        
        loader.startLoading(controller: self.parent ?? self)
        
        ServerConnection.loginOrRefreshToServer(["Company" : company,"Code":usercode,"Password":password]) { (serverResponse) -> Void in
    
            self.loader.stopLoading()

            do{
                if(serverResponse.error != nil) {
                    self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
                    self.loader.stopLoading()
                }
                
                if serverResponse.data != nil {
                    
                    let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                    
                    if(json["error"] == nil) {
                        
                        SignersManager.shared.addSigner(json: json)
                        self.dismiss(animated: true, completion: nil)
                        
                    } else {
                        let messageMex = json["error"] != nil ? json["error_description"] as! String : ""
                        self.appDelegate.displayMessage(Title: "Error", Message: messageMex)
                    }
                }
            } catch {
                self.appDelegate.displayMessage(Title: "Error", Message: "Cannot parse response")
            }
            
        }
        
    }
    
    func textField(_ textField: UITextField, shouldChangeCharactersIn range: NSRange, replacementString string: String) -> Bool {
        
        if !(textField.isEqual(userCodeTextField) ||  textField.isEqual(passwordTextField))  { return true }
        
        guard let textFieldText = textField.text,
            let rangeOfTextToReplace = Range(range, in: textFieldText) else {
                return false
        }
        let substringToReplace = textFieldText[rangeOfTextToReplace]
        let count = textFieldText.count - substringToReplace.count + string.count
        return count <= 60
    }
}
