
//
//  UIApplication.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 12/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

@objc(MyApplication) class MyApplication: UIApplication, UIApplicationDelegate {
    
    var timeToExpire = 0.0
    var displayAlert : UIAlertController?
    var isWarningPresented = false
    let timeBeforeTokenExpires = 120.0 /* 2 minutes */
    var isRefreshing = false
    var requestToOpenInstance = false
    var needToUpdateTokens = false
    
    var timer : Timer?
    var inactivityTimer : Timer?
    
    let defaultInactivityTime = 900.0 /* 15 minutes */
    
    var requestedUrlInfo : Dictionary<String, String>?
    
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
            
            if(expireDate < Date() && !isWarningPresented) {
                self.displayError(Title: "Session Expired", Message: "Your session is expired, you'll be redirected to login section.")
            } else {
                self.refreshToken(expireDate)
            }
        }
    }
    
    func refreshToken(_ expireDate:Date) {
    
        timeToExpire = expireDate.timeIntervalSinceNow
        
        if(timeToExpire <= timeBeforeTokenExpires && !isWarningPresented && !isRefreshing) {
        
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
                            
                            if let controller = self.getInstanceWebView() {
                                controller.refreshTokensByJSInjection(accessToken: UserDefaults.standard.value(forKey: "access_token") as! String, refreshToken: UserDefaults.standard.value(forKey: "refresh_token") as! String)
                            }
                            
                        } else {
                            
                            self.displayError(Title: "Error", Message: "Something has gone wrong, you'll be redirected to login section.")
                            
                            self.isRefreshing = false
                        }
                    }
                } catch {
                    print("Error deserializing JSON: \(error)")
                    self.displayError(Title: "Error", Message: "Something has gone wrong, you'll be redirected to login section.")
                }
                
            }
            
        }
        
    }
    
    private func getInstanceWebView() -> InstanceViewController? {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if topController.isKind(of: TabViewController.self)
            {
                
                if((topController as! TabViewController).selectedIndex == 1) {
                    if let navigationController = (topController as! TabViewController).viewControllers![(topController as! TabViewController).selectedIndex] as? UINavigationController {
                        if(navigationController.childViewControllers.count > 2) {
                            print(navigationController.childViewControllers.debugDescription)
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
                
                if let controller = (topController as! TabViewController).viewControllers![(topController as! TabViewController).selectedIndex] as? InstancesSearchViewController {
                    requestToOpenInstance = false
                    controller.performSegue(withIdentifier: "openInstanceDirectly", sender: self)
                }
            }
        }
    }
    
    func logoutAndGoToLogin()
        
    {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if topController.isKind(of: TabViewController.self)
            {
        
                if((topController as! TabViewController).selectedIndex == 1) {
                    if let navigationController = (topController as! TabViewController).viewControllers![(topController as! TabViewController).selectedIndex] as? UINavigationController {
                        if(navigationController.childViewControllers.count > 2) {
                            navigationController.popViewController(animated: false)
                        }
                    }
                }
            }
        }
        UserDefaults.standard.setValue(false, forKey: "logged")
        let storyboard = UIStoryboard(name: "Main", bundle: nil)
        let homeVC = storyboard.instantiateViewController(withIdentifier: "FirstCheck")
        UIApplication.shared.keyWindow?.rootViewController = homeVC
    }
    
    func displayError(Title title:String, Message message:String) {
        
        self.displayAlert = UIAlertController(title: title, message: message, preferredStyle: .alert)
        self.displayAlert!.addAction(UIAlertAction(title: "Ok", style: .destructive, handler: { (action) -> Void in
            self.isWarningPresented = false
            self.logoutAndGoToLogin()
        }))
        
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if topController.isKind(of: UIViewController.self)
            {
                topController.present(self.displayAlert!, animated: false, completion: nil)
                self.isWarningPresented = true
            }
        }
        
    }
    
    func refreshInactivityTimer(timeToExpireActivity: Double) {
        let logged = NSString(string:UserDefaults.standard.string(forKey: "logged")!).boolValue
        
        if(logged){
            if let timer = inactivityTimer {
                timer.invalidate()
            }
            inactivityTimer = Timer.scheduledTimer(timeInterval: timeToExpireActivity, target: UIApplication.shared, selector: #selector(MyApplication.inactivityHasDetected), userInfo: nil, repeats: true)
        }
    }
    
    @objc func inactivityHasDetected() {
        if let timer = inactivityTimer {
            timer.invalidate()
        }
        
        self.logoutAndGoToLogin()
    }
    
    func goBackGround(){
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




