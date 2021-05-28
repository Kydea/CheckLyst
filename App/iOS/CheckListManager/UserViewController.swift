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

}
