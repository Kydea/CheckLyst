//
//  UpdateManager.swift
//  Checklyst
//
//  Created by Matematici Developer on 12/05/2020.
//  Copyright © 2020 Johnson&Johnson. All rights reserved.
//

import UIKit

class UpdateManager: NSObject {
    
    fileprivate let userAppVersionKey : String = "AppVersion"
    fileprivate let currentAppVersionKey : String = "CFBundleShortVersionString"
    
    fileprivate static var instance : UpdateManager?
    
    static var shared : UpdateManager {
        
        if (instance == nil) {
            instance = UpdateManager()
        }
        return instance!
    }
    
    var currentAppVersion : Int {
        return Int((Bundle.main.infoDictionary![currentAppVersionKey] as! String).replacingOccurrences(of: ".", with: ""))!
    }
    
    var userAppVersion : Int {
        get {
           return UserDefaults.standard.integer(forKey: userAppVersionKey)
        }
    }
    
    fileprivate let managedObjectContext = ((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)
    
    fileprivate func updateUserAppVersion() {
        UserDefaults.standard.set(currentAppVersion, forKey: userAppVersionKey)
    }
    
    func startUpdates() {
        if ((self.userAppVersion == 0 || self.userAppVersion == 200) && self.currentAppVersion == 220) {
            updateFrom2_0_0to2_2_0()
        }
        updateUserAppVersion()
    }
    
    // MARK: - Update from 2.0.0 to 2.2.0
    
    fileprivate func updateFrom2_0_0to2_2_0() {
    
        let chooseDictionary = [
            "https://checklyst.jnj.com":"https://checklystapi.jnj.com",
            "http://10.157.83.244":"http://10.157.83.252",
            "https://checklystqa.jnj.com":"https://checklystqaapi.jnj.com",
            "https://checklystdev.jnj.com":"https://checklystdevapi.jnj.com",
            "http://clm.matematici-aws.com":"http://api.clm.matematici-aws.com"]
       
        if Server.any(managedObjectContext: managedObjectContext) && !Server.isSetServer(managedObjectContext: managedObjectContext) {
           
            let server = Server.getServers(managedObjectContext)!.first!
            
            if let storedApiServer = chooseDictionary[server.serverFrontEndUrl!] {
                Server.setNewServer(urlFrontEnd: server.serverFrontEndUrl!, apiUrl: storedApiServer, managedObjectContext: self.managedObjectContext)
            }
            
            if let controller = (UIApplication.shared as! MyApplication).searchPresentedController(controllerClass: UIViewController.self) {
                
                for childController in controller.childViewControllers {
                    if let loginViewController = childController as? LoginViewController {
                        loginViewController.viewWillAppear(false)
                    }
                }
            }
        }
    }

}
