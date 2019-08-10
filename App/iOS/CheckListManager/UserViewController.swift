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
    
    let app = (UIApplication.shared as! MyApplication)
    
    override func viewDidLoad() {
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem)
        userCodeTextField.isEnabled = false
        userCodeTextField.text = UserDefaults.standard.string(forKey: "userCode")
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        activityIndicator.hidesWhenStopped = true
    }
    
    //TODO prova a cambiare l'hook su viewWillApper, magari è megli visivamente
    override func viewDidAppear(_ animated: Bool) {
        super.viewDidAppear(animated)
        if (app.requestToOpenInstance) {
            app.openInstance()
        }
    }
    
    
    
    @IBAction func logoutFromPortal(_ sender: AnyObject) {
        
        activityIndicator.startAnimating()
        
        ServerConnection.logoutFromServer(["data":["refreshToken":UserDefaults.standard.value(forKey: "refresh_token") as! String]]){ (serverResponse) -> Void in
            if(serverResponse.data != nil){
                self.app.logoutAndGoToLogin()
            }
            else
            {
                let alert = UIAlertController(title: "Error", message: "Connection error to the server", preferredStyle: .alert)
                alert.addAction(UIAlertAction(title: "Ok", style: .destructive, handler: nil))
                self.present(alert, animated: false, completion: nil)
            }
            self.activityIndicator.stopAnimating()
        }
    }

}
