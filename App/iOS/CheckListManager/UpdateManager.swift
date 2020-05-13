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
        
        if (instance != nil) {
            return instance!
        }
        
        instance = UpdateManager()
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
        if ((self.userAppVersion == 0 || self.userAppVersion == 200) && self.currentAppVersion == 210) {
            updateFrom2_0_0to2_1_0()
        }
        updateUserAppVersion()
    }
    
    // MARK: - Update from 2.0.0 to 2.1.0
    
    fileprivate func updateFrom2_0_0to2_1_0() {
        if Server.any(managedObjectContext: managedObjectContext) && !Server.isSetServer(managedObjectContext: managedObjectContext) {
           
            let server = Server.getServers(managedObjectContext)!.first!
            ServerConnection.getApiServerRequest(server: server.serverFrontEndUrl!) { (serverResponse) -> Void in
                
                if (serverResponse.error == nil && serverResponse.data != nil) {
                    do {
                        
                        let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                        
                        let apiServer = (json["API"] as! String)
                        
                        Server.setNewServer(urlFrontEnd: server.serverFrontEndUrl!, apiUrl: apiServer, managedObjectContext: self.managedObjectContext)
                        
                        if let controller = (UIApplication.shared as! MyApplication).searchPresentedController(controllerClass: UIViewController.self) {
                            
                            for childController in controller.childViewControllers {
                                if let loginViewController = childController as? LoginViewController {
                                    loginViewController.viewWillAppear(false)
                                }
                            }
                        }
                        
                    } catch {
                        
                        print("Error deserializing JSON: \(error)")
                        
                    }
                }
            }
        }
    }

}
