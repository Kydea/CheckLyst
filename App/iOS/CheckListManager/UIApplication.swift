
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
    let timeBeforeTokenExpires = 120.0
    var isRefreshing = false
    
    var activity: Bool = true
    
    var timer : Timer?
    
    
    override func sendEvent(_ event: UIEvent) {
        
        if event.type != .touches {
            super.sendEvent(event)
            return
        }
        
        var restartTimer = true
        
        if let touches = event.allTouches {
            for touch in touches.enumerated() {
                if touch.element.phase != .cancelled && touch.element.phase != .ended {
                    restartTimer = false
                    break
                }
            }
        }
        
        
        if restartTimer {
            activity = true
        }
        super.sendEvent(event)
    }
    
    @objc func timerAction() {
        
        let logged = NSString(string:UserDefaults.standard.string(forKey: "logged")!).boolValue
        
        if(logged)
            
        {
            let expireDate = (UserDefaults.standard.value(forKey: "token_expires") as! Date)//.addingTimeInterval(-1665)
            
            if(expireDate < Date() && !isWarningPresented) {
                print("expired Token")
                self.displayError(Title: "Session Expired", Message: "Your session is expired, you'll be redirected to login section.")
            } else {
                self.refreshToken(expireDate)
            }
        }
    }
    
    func refreshToken(_ expireDate:Date) {
    
        timeToExpire = expireDate.timeIntervalSinceNow
        
        print(timeToExpire)
        
        if(timeToExpire <= timeBeforeTokenExpires && !isWarningPresented && !isRefreshing) {
            
            print("refreshing user token")
            
            isRefreshing = true
            
            ServerConnection.loginOrRefreshToServer(["Refresh_Token" : UserDefaults.standard.value(forKey: "refresh_token") as! String]) { (serverResponse) -> Void in
                do{
                    if serverResponse.data != nil
                    {
                        let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                        
                        print ("data received = \(json)")
                        
                        if(json["error"] == nil)
                            
                        {
                            UserDefaults.standard.setValue(json["access_token"], forKey: "access_token")
                            
                            UserDefaults.standard.setValue(Formatter.instance.dateFromTimeString(json[".expires"] as! String), forKey: "token_expires")
                            
                            self.isRefreshing = false
                            
                        } else {
                    
                            print("##### REFRESH ERROR #####")
                            print("access token = \(String(describing: UserDefaults.standard.value(forKey: "access_token")))")
                            print("refresh token = \(String(describing: UserDefaults.standard.value(forKey: "refresh_token")))")
                            
                            self.displayError(Title: "Error", Message: "Something has gone wrong, you'll be redirected to login section.")
                        }
                    }
                } catch {
                    print("Error deserializing JSON: \(error)")
                    self.displayError(Title: "Error", Message: "Something has gone wrong, you'll be redirected to login section.")
                }
                
            }
            
        }
        
    }
    
    func logoutAndGoToLogin()
        
    {
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
    
    func goBackGround(){
        timer!.invalidate()
        timeToExpire = 0
    }
    
    
    func goForeGround()
    {
        timer = Timer.scheduledTimer(timeInterval: 1.0, target: UIApplication.shared, selector: #selector(MyApplication.timerAction), userInfo: nil, repeats: true)
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




