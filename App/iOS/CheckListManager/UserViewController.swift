//
//  UserViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 23/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

class UserViewController: UIViewController {
    
    private let userSetPinSegueIdentifier = "userSetPinSegue"
    
    @IBOutlet var userCodeTextField: UITextField!
    
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    
    let appDelegate = (UIApplication.shared as! MyApplication)
    
    override func viewDidLoad() {
        
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem)
        userCodeTextField.isEnabled = false
        userCodeTextField.text = UserDefaults.standard.string(forKey: "userCode")
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        activityIndicator.hidesWhenStopped = true
    
    }
    
    override func viewDidAppear(_ animated: Bool) {

        super.viewDidAppear(animated)

        if (appDelegate.requestToOpenInstance) {
            appDelegate.openInstance()
        }
        
        if (appDelegate.requestToOpenSearch) {
            appDelegate.openSearch()
        }

    }
    
    
    
    @IBAction func logoutFromPortal(_ sender: AnyObject) {
        
        guard let refreshToken = UserDefaults.standard.value(forKey: "refresh_token") as? String else {
            self.appDelegate.logoutAndGoToLogin()
            return
        }
        
        activityIndicator.startAnimating()
        
        ServerConnection.logoutFromServer(["data":["refreshToken":refreshToken]]){ (serverResponse) -> Void in
        
            if(serverResponse.data != nil){
                
                self.appDelegate.logoutAndGoToLogin()
            
            } else {
            
                self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
            
            }
        
            self.activityIndicator.stopAnimating()
        
        }
    }
    
    @IBAction func setPinButtonAction(_ sender: Any) {
        performSegue(withIdentifier: userSetPinSegueIdentifier, sender: nil)
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        
        if let controller = segue.destination as? SignerPinViewController {
            let user = Signer(
                usercode: UserDefaults.standard.string(forKey: "userCode")!,
                accessToken: UserDefaults.standard.string(forKey: "access_token")!,
                refreshToken: UserDefaults.standard.string(forKey: "refresh_token")!,
                roles: UserDefaults.standard.string(forKey: "roles")!,
                name: UserDefaults.standard.string(forKey: "name")!,
                accessTokenExpiration: UserDefaults.standard.date(forKey: "token_expires")?.timeIntervalSinceNow ?? 0
            )
            controller.signer = user
        }
    }
    
}
