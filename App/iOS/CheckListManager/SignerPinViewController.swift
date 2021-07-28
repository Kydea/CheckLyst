//
//  SignerPinViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 10/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import UIKit

class SignerPinViewController: UIViewController, UITextFieldDelegate {

    @IBOutlet var loginView: UIView!
    @IBOutlet var userCodeTextField: UITextField!
    @IBOutlet var setPinTextField: UITextField!
    @IBOutlet var confirmPinTextField: UITextField!
    @IBOutlet var passwordTextField: UITextField!
    @IBOutlet var closeButton: UIButton!
    @IBOutlet var logoutButton: UIButton!

    let appDelegate = (UIApplication.shared as! MyApplication)
    let loader: LoadingViewController = LoadingViewController.shared

    
    var signer: Signer?
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        passwordTextField.delegate = self
        setPinTextField.delegate = self
        confirmPinTextField.delegate = self

        setupUI()
    }

    override func viewWillAppear(_ animated: Bool) {
        userCodeTextField.text = signer?.code
    }
    
    private func setupUI() {
    
        let isMainUser = signer?.code == UserDefaults.standard.string(forKey: "userCode")
        
        logoutButton.isHidden = isMainUser
        
        userCodeTextField.isEnabled = false
        
    }
    
    func changePin() {
        
        guard let pin = self.setPinTextField.text, !pin.isEmpty else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Insert a PIN")
            return
        }
        
        guard let confirmedPin = self.confirmPinTextField.text, !confirmedPin.isEmpty else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Confirm the PIN")
            return
        }
        
        guard let password = self.passwordTextField.text, !password.isEmpty else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Missing password")
            return
        }
        
        if pin.count < 6 {
            self.appDelegate.displayMessage(Title: "Error", Message: "PIN's length must be greater than 5 characters")
            return
        }
        
        if pin != confirmedPin {
            self.appDelegate.displayMessage(Title: "Error", Message: "PIN and confirm PIN must contain the same values")
            return
        }
        
        loader.startLoading(controller: self.parent ?? self)
        
        ServerConnection.setPin(["Pin" : pin,"Password":password], accessToken: signer!.accessToken) { (serverResponse) -> Void in
            self.loader.stopLoading(controller: self) { [weak self] in

                do{
                    if(serverResponse.error != nil) {
                        self?.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
                    }
                    
                    if serverResponse.data != nil {
                        
                        if(serverResponse.data?.count == 0) {
                            self?.dismiss(animated: true, completion: nil)
                        } else {
                        
                            let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                        
                            let messages = json["messages"] as? [String]
                            let messageMex = messages?[0] as? String ?? ""
                            self?.appDelegate.displayMessage(Title: "Error", Message: messageMex)
                        }
                    }
                } catch {
                    self?.appDelegate.displayMessage(Title: "Error", Message: "Cannot parse response")
                }
            }
            
        }
        
    }
    
    @IBAction func logoutButtonAcrion(_ sender: UIButton) {
        guard let signer = self.signer else { return }
        SignersManager.shared.deleteSinger(signer)
        self.dismiss(animated: true, completion: nil)
    }
    
    @IBAction func setPinButtonAction(_ sender: UIButton) {
        changePin()
    }
    
    @IBAction func closeButtonAction(_ sender: UIButton) {
        self.dismiss(animated: true, completion: nil)
    }
    
    func textField(_ textField: UITextField, shouldChangeCharactersIn range: NSRange, replacementString string: String) -> Bool {
        
        if (textField.isEqual(setPinTextField) || textField.isEqual(confirmPinTextField)) {
            let allowedCharacters = CharacterSet.decimalDigits
            let characterSet = CharacterSet(charactersIn: string)
            return allowedCharacters.isSuperset(of: characterSet)
        }
        
        guard let textFieldText = textField.text,
            let rangeOfTextToReplace = Range(range, in: textFieldText) else {
                return false
        }
        
        let substringToReplace = textFieldText[rangeOfTextToReplace]
        let count = textFieldText.count - substringToReplace.count + string.count
        return count <= 60
    }
}
