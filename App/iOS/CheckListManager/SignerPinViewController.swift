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
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    
    var signer: Signer?
    
    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
    }
    
    private func setupUI() {
        
        userCodeTextField.isEnabled = false
        
        activityIndicator.hidesWhenStopped = true
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
    }
    
    override func viewWillAppear(_ animated: Bool) {
        userCodeTextField.text = signer?.code
    }

    @IBAction func logoutButtonAcrion(_ sender: UIButton) {
        guard let signer = self.signer else { return }
        SignersManager.shared.deleteSinger(signer)
        self.dismiss(animated: true, completion: nil)
    }
    
    @IBAction func setPinButtonAction(_ sender: UIButton) {
    }
    
    @IBAction func closeButtonAction(_ sender: UIButton) {
        self.dismiss(animated: true, completion: nil)
    }
}
