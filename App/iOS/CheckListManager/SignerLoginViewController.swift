//
//  SignerLoginViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 10/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import UIKit

class SignerLoginViewController: UIViewController {

    @IBOutlet var loginView: UIView!
    @IBOutlet var userCodeTextField: UITextField!
    @IBOutlet var passwordTextField: UITextField!
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    
    let appDelegate = (UIApplication.shared as! MyApplication)

    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
    }

    private func setupUI() {
        activityIndicator.hidesWhenStopped = true
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
    }
    
    @IBAction func closeButtonAction(_ sender: UIButton) {
        self.dismiss(animated: true, completion: nil)
    }
    
    @IBAction func signButtonAction(_ sender: UIButton) {
        performSign()
    }
    
    func performSign() {
        
        activityIndicator.startAnimating()
        self.view.isUserInteractionEnabled = false
        
        guard let usercode = self.userCodeTextField.text else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Insert a user code")
            return
        }
        
        guard let password = self.passwordTextField.text else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Insert a password")
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
            self.activityIndicator.stopAnimating()
            self.view.isUserInteractionEnabled = true
            return
        }
        
        ServerConnection.loginOrRefreshToServer(["Company" : company,"Code":usercode,"Password":password]) { (serverResponse) -> Void in
    
            do{
                if(serverResponse.error != nil) {
                    self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
                    self.activityIndicator.stopAnimating()
                    self.view.isUserInteractionEnabled = true
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
                    self.activityIndicator.stopAnimating()
                    self.view.isUserInteractionEnabled = true
                }
            } catch {
                self.appDelegate.displayMessage(Title: "Error", Message: "Cannot parse response")
                self.activityIndicator.stopAnimating()
                self.view.isUserInteractionEnabled = true
            }
            
        }
        
    }
}
