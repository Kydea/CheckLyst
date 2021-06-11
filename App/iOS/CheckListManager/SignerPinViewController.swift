//
//  SignerPinViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 10/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import UIKit

class SignerPinViewController: UIViewController {

    @IBOutlet var loginView: UIView!
    @IBOutlet var userCodeTextField: UITextField!
    @IBOutlet var setPinTextField: UITextField!
    @IBOutlet var confirmPinTextField: UITextField!
    @IBOutlet var passwordTextField: UITextField!
    @IBOutlet var closeButton: UIButton!
    @IBOutlet var activityIndicator: UIActivityIndicatorView!

    let appDelegate = (UIApplication.shared as! MyApplication)
    
    var signer: Signer?
    
    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
    }

    override func viewWillAppear(_ animated: Bool) {
        userCodeTextField.text = signer?.code
    }
    
    private func setupUI() {
        
        userCodeTextField.isEnabled = false
        
        activityIndicator.hidesWhenStopped = true
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
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
        
        activityIndicator.startAnimating()
        self.view.isUserInteractionEnabled = false
        
        ServerConnection.setPin(["Pin" : pin,"Password":password], accessToken: signer!.accessToken) { (serverResponse) -> Void in
    
            do{
                if(serverResponse.error != nil) {
                    self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
                    self.activityIndicator.stopAnimating()
                    self.view.isUserInteractionEnabled = true
                }
                
                if serverResponse.data != nil {
                    
                    if(serverResponse.data?.count == 0) {
                        self.dismiss(animated: true, completion: nil)
                    } else {
                    
                        let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                    
                        let messages = json["messages"] as? [String]
                        let messageMex = messages?[0] as? String ?? ""
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
}
