//
//  FirstCheckViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 23/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

class FirstCheckViewController: UIViewController {
        
    let appDelegate = UIApplication.shared as! MyApplication
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
    }
    
    override func viewDidAppear(_ animated: Bool) {
        
        super.viewDidAppear(animated)
        // both flags are needed for debug behaviour, searchPresentedController is needed
        // in release mode, without this control there is a duplicated LoginViewController
        if appDelegate.isInactivityCatched || appDelegate.isTokenExpired || appDelegate.searchPresentedController(controllerClass: UINavigationController.self) != nil { return }
        
        let logged = UserDefaults.standard.existKey("access_token")
        let segue = (logged) ? "tabBarSegue" : "loginSegue"
        
        self.performSegue(withIdentifier: segue, sender: self)

    }
    
    
    

}
