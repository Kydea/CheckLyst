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
    
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
    }
    
    

    
    override func viewDidAppear(_ animated: Bool) {
        
        super.viewDidAppear(animated)
        
        let logged = UserDefaults.standard.value(forKey: "logged") as! Bool 
        let segue = (logged) ? "tabBarSegue" : "loginSegue"
    
        self.performSegue(withIdentifier: segue, sender: self)

    }
    
    
    

}
