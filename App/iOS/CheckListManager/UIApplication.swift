
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
    var isTokenExpired = false
    var requestToOpenInstance = false
    var requestToOpenSearch = false
    var requestToUpdateServer = false
    
    var timer : Timer?
    var inactivityTimer : Timer?
    var inactivityStartingTime = Date()
    
    let defaultInactivityTime = 900.0 /* 15 minutes */
    let showCountdownThreshold = 60.0
    
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
            inactivityStartingTime = Date()
            CountDownViewController.shared.reset()
        }
        super.sendEvent(event)
    }
    
    @objc func timerAction() {
        UpdateUserTokenIfNeeded()
        UpdateSignersTokenIfNeeded()
    }
    
    func UpdateSignersTokenIfNeeded() {
        let logged = UserDefaults.standard.existKey("access_token")
        
        guard logged else { return }
        
        for signer in SignersManager.shared.signers {
            timeToExpire = Date(timeIntervalSince1970: signer.accessTokenExpiration).timeIntervalSinceNow;
            
            if timeToExpire <= timeBeforeTokenExpires && !self.isWarningPresented && !isRefreshing {
                
                #if DEBUG
                print("Refreshing \(signer.code) access token at: \(Formatter.instance.stringFromTestDate(Date()))")
                #endif
            
                self.refreshToken(signer.refreshToken) { json in
                    SignersManager.shared.updateSigner(json: json)
                    
                    if let controller = self.getViewController(ofKind: InstanceViewController.self) as? InstanceViewController {
                        controller.setOrRefreshSigners(SignersManager.shared.signers)
                    }
                }
            }
        }
    }
    
    func UpdateUserTokenIfNeeded() {
        let logged = UserDefaults.standard.existKey("access_token")
        
        guard logged else { return }
        
        guard let expireDate = UserDefaults.standard.date(forKey: "token_expires") else {
            return
        }
        
        timeToExpire = expireDate.timeIntervalSinceNow;
        
        guard timeToExpire <= timeBeforeTokenExpires && !self.isWarningPresented && !isRefreshing else {
            return
        }
        
        #if DEBUG
        print("Refreshing \(UserDefaults.standard.string(forKey: "userCode") ?? "") access token at: \(Formatter.instance.stringFromTestDate(Date()))")
        #endif
        
        self.refreshToken(UserDefaults.standard.value(forKey: "refresh_token") as! String) { json in
            
            let accessToken = json["access_token"] as! String
            let tokenExpires = Date() + (json["expires_in"] as! TimeInterval)
            let refreshToken = UserDefaults.standard.string(forKey: "refresh_token")!

            UserDefaults.standard.setValue(accessToken, forKey: "access_token")
            UserDefaults.standard.setValue(tokenExpires, forKey: "token_expires")
            
            if let controller = self.getViewController(ofKind: InstanceViewController.self) as? InstanceViewController {
                controller.refreshTokensByJSInjection(accessToken: accessToken, refreshToken: refreshToken )
            }
            
            if let controller = self.getViewController(ofKind: UserManualViewController.self) as? UserManualViewController {
                controller.refreshTokensByJSInjection(accessToken: accessToken, refreshToken: refreshToken)
            }
        }
    }
    
    func refreshToken(_ refreshToken:String, refreshAction: @escaping (_ json: [String: Any]) -> Void) {
        
        isRefreshing = true
        
        ServerConnection.loginOrRefreshToServer(["Refresh_Token" : refreshToken]) { (serverResponse) -> Void in
            do{
                if serverResponse.data != nil
                {
                    let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                    
                    if(json["error"] == nil)
                    
                    {
                        refreshAction(json)
                        self.isRefreshing = false
                        
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
    
    private func getViewController<T: UIViewController>(ofKind kind:AnyClass) -> T? {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if let tabController = topController as? TabViewController {
                if let navigationControllers = tabController.viewControllers {
                    for navigationController in navigationControllers {
                        if let nController = navigationController as? UINavigationController {
                            for controller in nController.childViewControllers {
                                if controller.isKind(of: kind.self) {
                                    return controller as? T
                                }
                            }
                        }
                    }
                }
            }
        }
        
        return nil
    }
    
    func goToSearch() {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
            
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if topController.isKind(of: TabViewController.self)
            {
                var isPresented = true
                if ((topController as! TabViewController).selectedIndex != 1) {
                    (topController as! TabViewController).selectedIndex = 1
                    isPresented = false
                }
                
                let tabBarController = (topController as! TabViewController)
                
                if let instanceController = tabBarController.viewControllers?[1].childViewControllers.last as? InstanceViewController {
                    instanceController.closeInstance() {
                        let navController = tabBarController.viewControllers?[1] as? UINavigationController
                        navController?.popToRootViewController(animated: false)
                    }
                } else {
                    let navController = tabBarController.viewControllers?[1] as? UINavigationController
                    navController?.popToRootViewController(animated: false)
                }
                
                if let instancesSearcController = tabBarController.viewControllers![1].childViewControllers[0] as? InstancesSearchViewController {
                    
                    if isPresented { instancesSearcController.viewDidAppear(false) }
                }

            }
        }
    }
    
    func openInstance(instancesSearchController: InstancesSearchViewController?) {
       
        self.requestToOpenInstance = false
        
        if let prototype = requestedUrlInfo?["prototype"] {
            instancesSearchController?.checkCreate(prototype: prototype) {
                instancesSearchController?.performSegue(withIdentifier: "openInstanceDirectly", sender: self)
            }
        }
        
        if requestedUrlInfo?["code"] != nil {
            
            instancesSearchController?.performSegue(withIdentifier: "openInstanceDirectly", sender: self)
        }
        
    }
    
    func openSearch() {
        if let tabBarController = searchPresentedController(controllerClass: TabViewController.self) as? TabViewController {
            var isPresented = true
            if tabBarController.selectedIndex != 1 {
                tabBarController.selectedIndex = 1
                isPresented = false
            }
            
            if let instanceController = tabBarController.viewControllers?[1].childViewControllers.last as? InstanceViewController {
                instanceController.closeInstance()
            }
            
            if let navController = tabBarController.viewControllers?[1] as? UINavigationController, navController.childViewControllers.count > 1 {
                navController.popToRootViewController(animated: false)
            }
            
            if let instancesSearcController = tabBarController.viewControllers![1].childViewControllers[0] as? InstancesSearchViewController {
                instancesSearcController.loadFilters()
                
                if isPresented { instancesSearcController.viewDidAppear(false) }
            }
            
            requestToOpenSearch = false
        }
    }
    
    
    func setupNewServer(_ server:String, managedObjectContext: NSManagedObjectContext) {
        
        let logged = UserDefaults.standard.existKey("access_token")
        
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
                
                defer {
                    NotificationCenter.default.post(name: .ServerHasBeenConfigured, object: nil)
                }
                
                if (serverResponse.error != nil) {
                    self.displayMessage(Title: "Error", Message: "Connection error to the server")
                } else if (serverResponse.data != nil) {
                   
                    do {
                        let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                        let apiServer = json["API"] as! String
                        
                        Server.addServerUrl(urlFrontEnd: server, managedObjectContext: managedObjectContext)
                        Server.setNewServer(urlFrontEnd: server, apiUrl: apiServer, managedObjectContext: managedObjectContext)
                        
                    } catch {
                        self.displayMessage(Title: "Error", Message: "Cannot parse response")
                        print("Error deserializing JSON: \(error)")
                    }
                }
            }
        }
    }
    
    func searchPresentedController(controllerClass: AnyClass? = nil) -> UIViewController? {
        if var topController = UIApplication.shared.keyWindow?.rootViewController {
        
            while let presentedViewController = topController.presentedViewController {
                topController = presentedViewController
            }
            
            if let searchClass = controllerClass {
                if topController.isKind(of: searchClass) {
                    return topController
                }
                return nil
            }
            
            return topController
        }
        
        return nil
    }
    
    func logoutAndGoToLogin() {
        
        UserDefaults.standard.setValue(nil, forKey: "access_token")
        UserDefaults.standard.setValue(nil, forKey: "refresh_token")
        UserDefaults.standard.setValue(nil, forKey: "roles")
        FilterManager.shared.clearFilters()
        SignersManager.shared.clearSigners()
        
        if let timer = timer {
            timer.invalidate()
        }
        
        if let timer = inactivityTimer {
            timer.invalidate()
        }
        
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
                
                tabBarController.dismiss(animated: true) {
                    if var topController = UIApplication.shared.keyWindow?.rootViewController {
                        
                        while let presentedViewController = topController.presentedViewController {
                            topController = presentedViewController
                        }
                        
                        if let firstCheckViewController = topController as? FirstCheckViewController {
                            firstCheckViewController.performSegue(withIdentifier: "loginSegue", sender: firstCheckViewController)
                        }
                    }
                }
            }
            
            if let firstCheckViewController = topController as? FirstCheckViewController {
                firstCheckViewController.performSegue(withIdentifier: "loginSegue", sender: firstCheckViewController)
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
                if (presentedViewController is LoadingViewController) { break }
                topController = presentedViewController
            }
            
            if topController.isKind(of: UIViewController.self)
            {
                topController.present(self.displayAlert!, animated: true, completion: nil)
                self.isWarningPresented = true
            }
        }
        
    }
    
    func startInactivityTimer() {
        let logged = UserDefaults.standard.existKey("access_token")
        
        if(logged && !isInactivityCatched){
            inactivityTimer?.invalidate()
            
            inactivityStartingTime = Date()
            
            inactivityTimer = Timer.scheduledTimer(timeInterval: 1.0, target: UIApplication.shared, selector: #selector(MyApplication.inactivityDetection), userInfo: nil, repeats: true)
        }
    }
    
    func startTokenTimer() {
        let logged = UserDefaults.standard.existKey("access_token")
        
        if(logged && !isTokenExpired) {
            
            timer?.invalidate()
            
            timer = Timer.scheduledTimer(timeInterval: 1.0, target: UIApplication.shared, selector: #selector(MyApplication.timerAction), userInfo: nil, repeats: true)
        }
    }
    
    @objc func inactivityDetection() {
        if (inactivityStartingTime.addingTimeInterval(inactivityInterval - showCountdownThreshold) > Date()) {
            return
        }
        
        if (inactivityStartingTime.addingTimeInterval(inactivityInterval - showCountdownThreshold) < Date() && !CountDownViewController.shared.isPresented) {
            CountDownViewController.shared.completition = { [weak self] in
                self?.inactivityTimer?.invalidate()
                self?.isInactivityCatched = true
                #if DEBUG
                let message = String(format: "No activity detected since %@", Formatter.instance.stringFromTestDate(self?.inactivityStartingTime ?? Date(), timeOnly: true))
                self?.displayMessage(Title: "Inactivity", Message: message) { [weak self] in
                    self?.logoutAndGoToLogin()
                    self?.isInactivityCatched = false
                }
                #else
                self?.logoutAndGoToLogin()
                self?.isInactivityCatched = false
                #endif
                
            }
            searchPresentedController()?.present(CountDownViewController.shared, animated: false)
        }
        
        CountDownViewController.shared.updateCountDown(time: inactivityStartingTime.addingTimeInterval(inactivityInterval).timeIntervalSinceNow)
    }
    
    func goBackGround(){
        self.getViewController(ofKind: InstanceViewController.self)?.viewWillDisappear(false);
        
        self.getViewController(ofKind: SignersViewController.self)?.viewWillDisappear(false)
        
        UserDefaults.standard.set(Date(), forKey: "backgroundTime")
        if let timer = timer {
            timer.invalidate()
        }
        if let timer = inactivityTimer {
            timer.invalidate()
        }
        timeToExpire = 0
    }
    
    func backFromBackGround() {
        let logged = UserDefaults.standard.existKey("access_token")

        if logged && !self.isTokenExpired && !self.isInactivityCatched {
            if let lastActiveTime = UserDefaults.standard.date(forKey: "backgroundTime") {
                
                if(lastActiveTime.addingTimeInterval(inactivityInterval) < Date()) {
                    self.isInactivityCatched = true
                    CountDownViewController.shared.completition = { [weak self] in
                        #if DEBUG
                        let message = String(format: "No activity detected since %@", Formatter.instance.stringFromTestDate(self?.inactivityStartingTime ?? Date(), timeOnly: true))
                            self?.displayMessage(Title: "Inactivity", Message: message) { [weak self] in
                                self?.logoutAndGoToLogin()
                                self?.isInactivityCatched = false
                            }
                        #else
                            self?.logoutAndGoToLogin()
                            self?.isInactivityCatched = false
                        #endif
                    }
                    CountDownViewController.shared.updateCountDown(time: inactivityStartingTime.timeIntervalSinceNow)
                } else {
                    self.startInactivityTimer()
                }
            }
            
            //avoid to run token sync if inactivity is catched, user is already logged out due to inactivity or just a warning is already displayed
            if self.isInactivityCatched || !UserDefaults.standard.existKey("access_token") || self.isWarningPresented { return }
            
            if let expireDate = UserDefaults.standard.date(forKey: "token_expires") {
                if(expireDate < Date()) {
                    self.isTokenExpired = true
                    self.displayMessage(Title: "Session Expired", Message: "Your session is expired, you'll be redirected to login section.") { [weak self]
                        () -> Void in
                        self?.logoutAndGoToLogin()
                        self?.isTokenExpired = false
                    }
                } else {
                    self.startTokenTimer()
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
        #if DEBUG
            let message = "App started at: \(Formatter.instance.stringFromTestDate(Date()))"
            try? message.write(to: FileManager.default.getDocumentsDirectory().appendingPathComponent("app_start.log"), atomically: true, encoding: .utf8)
        #endif
    }
}




