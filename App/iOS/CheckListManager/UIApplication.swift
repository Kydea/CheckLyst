
//
//  UIApplication.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 12/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit
import CoreData

@objc(MyApplication) class MyApplication: UIApplication, UIApplicationDelegate {
    
    var timeToExpire = 0.0
    var displayAlert : UIAlertController?
    var isWarningPresented = false
    let timeBeforeTokenExpires = 120.0 /* 2 minutes */
    var isRefreshing = false
    var isInactivityCatched = false
    var requestToOpenInstance = false
    var needToUpdateTokens = false
    
    var timer : Timer?
    var inactivityTimer : Timer?
    var inactivityStartingTime : Date?
    
    let defaultInactivityTime = 900.0 /* 15 minutes */
    
    var requestedUrlInfo : Dictionary<String, String>?
    
    var currentWebViewCreationTime: Date?
    
    var inactivityInterval : TimeInterval {
        return (UserDefaults.standard.value(forKey: "inactivityTimeoutSeconds") as? Double) ?? defaultInactivityTime
    }
    
    override func sendEvent(_ event: UIEvent) {
        
        if event.type != .touches {
            super.sendEvent(event)
            return
        }
        
        var restartTimer = false
        
        if let touches = event.allTouches {
            for touch in touches.enumerated() {
                if touch.element.phase != .cancelled && touch.element.phase != .ended {
                    restartTimer = true
                    break
                }
            }
        }
        
        
        if restartTimer {
            self.refreshInactivityTimer(timeToExpireActivity: inactivityInterval)
        }
        super.sendEvent(event)
    }
    
    @objc func timerAction() {
        
        let logged = NSString(string:UserDefaults.standard.string(forKey: "logged")!).boolValue
        
        if(logged)
        {
            let expireDate = UserDefaults.standard.value(forKey: "token_expires") as! Date
            if(expireDate < Date() && !self.isWarningPresented) {
                self.displayMessage(Title: "Session Expired", Message: "Your session is expired, you'll be redirected to login section.") {
                    () -> Void in
                    self.logoutAndGoToLogin()
                }
            } else {
                self.refreshToken(expireDate)
            }
        }
    }
    
    func refreshToken(_ expireDate:Date) {
    
        timeToExpire = expireDate.timeIntervalSinceNow;
        
        if(timeToExpire <= timeBeforeTokenExpires && !self.isWarningPresented && !isRefreshing) {
        
            isRefreshing = true
            
            ServerConnection.loginOrRefreshToServer(["Refresh_Token" : UserDefaults.standard.value(forKey: "refresh_token") as! String]) { (serverResponse) -> Void in
                do{
                    if serverResponse.data != nil
                    {
                        let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                       
                        if(json["error"] == nil)
                            
                        {
                            UserDefaults.standard.setValue(json["access_token"], forKey: "access_token")
                            
                            UserDefaults.standard.setValue(Date() + (json["expires_in"] as! TimeInterval), forKey: "token_expires")
                            
                            self.isRefreshing = false
                            self.needToUpdateTokens = true
                            
                            if let controller = self.getInstanceWebViewIfVisible() {
                                controller.refreshTokensByJSInjection(accessToken: UserDefaults.standard.value(forKey: "access_token") as! String, refreshToken: UserDefaults.standard.value(forKey: "refresh_token") as! String)
                            }
                            
                        } else {
                            
                            self.displayMessage(Title: "Error", Message: "Something has gone wrong while refreshing token: \(String(describing: json["error"])).\n You'll be redirected to login section.") {
                                () -> Void in
                                self.logoutAndGoToLogin()
                            }
                            
                            self.isRefreshing = false
                        }
                    } else if serverResponse.error != nil {
                        self.displayMessage(Title: "Error", Message: "Something has gone wrong while refreshing token: Connection error to the server.\n You'll be redirected to login section.") {
                            () -> Void in
                            self.logoutAndGoToLogin()
                        }
                    }
                } catch {
                    print("Error deserializing JSON: \(error)")
                    self.displayMessage(Title: "Error", Message: "Something has gone wrong while refreshing token: Cannot parse response.\n You'll be redirected to login section.") {
                        () -> Void in
                        self.logoutAndGoToLogin()
                    }
                }
                
            }
            
        }
        
    }
    
    private func getInstanceWebViewIfVisible() -> InstanceViewController? {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if topController.isKind(of: TabViewController.self)
            {
                
                if((topController as! TabViewController).selectedIndex == 1) {
                    if let navigationController = (topController as! TabViewController).viewControllers![(topController as! TabViewController).selectedIndex] as? UINavigationController {
                        if(navigationController.childViewControllers.count > 2) {
                            for controller in navigationController.childViewControllers {
                                if controller.isKind(of: InstanceViewController.self) {
                                    return controller as? InstanceViewController
                                }
                            }
                        }
                    }
                }
            }
        }
        return nil
    }
    
    func openInstance() {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if topController.isKind(of: TabViewController.self)
            {
                
                if ((topController as! TabViewController).selectedIndex != 1) {
                    (topController as! TabViewController).selectedIndex = 1
                }
                
                let tabBarController = (topController as! TabViewController)
                
                if let controller = tabBarController.viewControllers![tabBarController.selectedIndex].childViewControllers[0] as? InstancesSearchViewController {
                    requestToOpenInstance = false
                    controller.performSegue(withIdentifier: "openInstanceDirectly", sender: self)
                }
            }
        }
    }
    
    func setupNewServer(_ server:String, managedObjectContext: NSManagedObjectContext) {
        
        let logged = NSString(string:UserDefaults.standard.string(forKey: "logged")!).boolValue
        
        //needed to trigger segue before alert presentation, otherwise segue won't perform
        if let vc = self.searchPresentedController(controllerClass: FirstCheckViewController.self) {
            let segue = (logged) ? "tabBarSegue" : "loginSegue"
            vc.performSegue(withIdentifier: segue, sender: self)
        }
        
        self.displayMessage(Title: "Server Configuration", Message: "You are setting up a new server.\(logged ? "\nYou'll be redirected to login section." : "")\nAre you sure?", canDecline: true) { () -> Void in
            
            if(logged) {
                self.logoutAndGoToLogin()
            }
        
            NotificationCenter.default.post(name: .NewServerConfiguration, object: nil)
            
            ServerConnection.getApiServerRequest(server: server) { (serverResponse) -> Void in
                
                if (serverResponse.error != nil) {
                    self.displayMessage(Title: "Error", Message: "Connection error to the server")
                } else if (serverResponse.data != nil) {
                   
                    do {
                        let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                        let apiServer = json["API"] as! String
                        
                        Server.addServerUrl(urlFrontEnd: server, managedObjectContext: managedObjectContext)
                        Server.setNewServer(urlFrontEnd: server, apiUrl: apiServer, managedObjectContext: managedObjectContext)
                        
                        NotificationCenter.default.post(name: .ServerHasBeenConfigured, object: nil)
                        
                    } catch {
                        self.displayMessage(Title: "Error", Message: "Cannot parse response")
                        print("Error deserializing JSON: \(error)")
                    }
                }
            }
        }
    }
    
    func searchPresentedController(controllerClass: AnyClass?) -> UIViewController? {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
        
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if let searchClass = controllerClass {
                if topController.isKind(of: searchClass) {
                    return topController
                }
            }
           
        }
        
        return nil
    }
    
    func logoutAndGoToLogin()
        
    {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if topController.isKind(of: TabViewController.self)
            {
        
                let tabBarController = (topController as! TabViewController)
                
                if let navigationController = tabBarController.viewControllers![1] as? UINavigationController {
                        
                    for childController in navigationController.childViewControllers {
                        if let instanceController = childController as? InstanceViewController {
                            instanceController.closeInstance()
                        }
                    }
                }
                
                UserDefaults.standard.setValue(false, forKey: "logged")
                UserDefaults.standard.setValue(nil, forKey: "access_token")
                UserDefaults.standard.setValue(nil, forKey: "refresh_token")
                UserDefaults.standard.setValue(nil, forKey: "roles")
                
                tabBarController.dismiss(animated: true, completion: nil)
            }
        }
    }
    
    func displayMessage(Title title:String, Message message:String, canDecline:Bool = false, ConfirmHandler:(()->Void)? = nil) {
        
        self.displayAlert = UIAlertController(title: title, message: message, preferredStyle: .alert)
        
        self.displayAlert!.addAction(UIAlertAction(title: "Ok", style: .default, handler: { (action) -> Void in
            if let handler = ConfirmHandler {
                handler()
                self.isWarningPresented = false
            }
        }))
        
        if(canDecline) {
            self.displayAlert!.addAction(UIAlertAction(title: "Cancel", style: .default, handler: nil))
        }
        
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if topController.isKind(of: UIViewController.self)
            {
                topController.present(self.displayAlert!, animated: true, completion: nil)
                self.isWarningPresented = true
            }
        }
        
    }
    
    func refreshInactivityTimer(timeToExpireActivity: Double) {
        let logged = NSString(string:UserDefaults.standard.string(forKey: "logged")!).boolValue
        
        if(logged && !isInactivityCatched){
            if let timer = inactivityTimer {
                timer.invalidate()
            }
            inactivityTimer = Timer.scheduledTimer(timeInterval: timeToExpireActivity, target: UIApplication.shared, selector: #selector(MyApplication.inactivityHasDetected), userInfo: nil, repeats: true)
            inactivityStartingTime = Date()
        }
    }
    
    @objc func inactivityHasDetected() {
        if let timer = inactivityTimer {
            timer.invalidate()
        }
        isInactivityCatched = true
        let message = String(format: "No activity detected since %@", Formatter.instance.stringFromTestDate(inactivityStartingTime!, timeOnly: true))
        self.displayMessage(Title: "Inactivity", Message: message) { [weak self] in
            self?.logoutAndGoToLogin()
            self?.isInactivityCatched = false
        }
    }
    
    func goBackGround(){
        if let controller = getInstanceWebViewIfVisible() {
            controller.viewWillDisappear(false);
        }
        
        UserDefaults.standard.set(Date(), forKey: "backgroundTime")
        if let timer = timer {
            timer.invalidate()
        }
        if let timer = inactivityTimer {
            timer.invalidate()
        }
        timeToExpire = 0
    }
    
    
    func goForeGround()
    {
        timer = Timer.scheduledTimer(timeInterval: 1.0, target: UIApplication.shared, selector: #selector(MyApplication.timerAction), userInfo: nil, repeats: true)
    }
    
    func backFromBackGround() {
        let logged = NSString(string:UserDefaults.standard.string(forKey: "logged")!).boolValue
        
        timer = Timer.scheduledTimer(timeInterval: 1.0, target: UIApplication.shared, selector: #selector(MyApplication.timerAction), userInfo: nil, repeats: true)
        
        if(logged){
            if let lastActiveTime = UserDefaults.standard.value(forKey: "backgroundTime") as? Date {
                
                if(lastActiveTime.addingTimeInterval(inactivityInterval) < Date()) {
                    self.logoutAndGoToLogin()
                } else {
                    self.refreshInactivityTimer(timeToExpireActivity: lastActiveTime.addingTimeInterval(inactivityInterval).timeIntervalSince(Date()))
                }
            }
        }
    }
    
    func goBackToList() {
        DispatchQueue.main.async {
            if var topController = UIApplication.shared.keyWindow?.rootViewController {
                
                while let presentedViewController = topController.presentedViewController {
                    topController = presentedViewController
                }
                
                (topController.childViewControllers[1] as? UINavigationController)?.popToRootViewController(animated: true)
            }
        }
    }
    
    override init() {
        super.init()
        UserDefaults.standard.setValue(false, forKey: "logged")
        timer = Timer.scheduledTimer(timeInterval: 1.0, target: UIApplication.shared, selector: #selector(MyApplication.timerAction), userInfo: nil, repeats: true)
    }
}




