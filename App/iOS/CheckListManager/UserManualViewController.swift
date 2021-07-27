//
//  UserManualViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 27/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit
import WebKit

class UserManualViewController: UIViewController, WKNavigationDelegate {
    
    
    @IBOutlet var webView: WKWebView!
        
    let appDelegate = UIApplication.shared as! MyApplication
    
    var helpLoaded : Bool = false
    
    var creationTime: Date?
    var currentAccessToken: String?
    
    let loader: LoadingViewController = LoadingViewController.shared
    
    override func viewDidLoad()
        
    {
        super.viewDidLoad()
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
        
        creationTime = Date()
        currentAccessToken = (UserDefaults.standard.value(forKey: "access_token") as! String)
        
        webView.navigationDelegate = self
        
        helpLoaded = false
        
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(true)
        if(!helpLoaded) {
            
            let fullUrl = ServerConnection.baseFrontEndUrl + "/Help/App?AppUser="+(UserDefaults.standard.string(forKey: "userCode"))!+"&d=" + String(Int64(Date().timeIntervalSince1970 * 1000))
            
            let url = URL(string: fullUrl)
            
            let request = NSMutableURLRequest(url: url!)
            
            request.httpMethod = "GET"
            
            request.timeoutInterval = 10
            
            //For testing -> gives 10 seconds to open safari inspector
            /*DispatchQueue.main.asyncAfter(deadline: .now() + .seconds(10)) {
                self.webView.load(request as URLRequest)
            }*/
            
            webView.load(request as URLRequest)
            
            self.lockView()
            
        }
        
    }
   
    override func viewDidLayoutSubviews() {
        
        webView.frame = CGRect(x: 0, y: 10, width: self.view.frame.size.width, height: self.view.frame.size.height)
        
    }
    
    
    func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
        unlockView()
        
        self.webView.isHidden = false
        
        helpLoaded = true
        
        webView.evaluateJavaScript("window.location.href") {
            (response, error) -> Void in
            if let value = response as? String {
                if(value.contains("Security/Login")) {
                    webView.isHidden = true
                    self.helpLoaded = false
                    #if DEBUG
                        let message = String(format: "UserManualViewController\nCurrent WKWebView creationTime: %@\nLogout WKWebView creationTime: %@\nCurrent accessToken: %@\nLogout WKWebView accessToken: %@\n", Formatter.instance.stringFromTestDate( self.appDelegate.currentWebViewCreationTime!), Formatter.instance.stringFromTestDate(self.creationTime!), (UserDefaults.standard.value(forKey: "access_token") as? String) ?? "nil", self.currentAccessToken ?? "nil")
                        try? message.write(to: FileManager.default.getDocumentsDirectory().appendingPathComponent("error.log"), atomically: true, encoding: .utf8)
                        self.appDelegate.displayMessage(Title: "Security Error", Message: message) { [weak self] in
                            self?.appDelegate.logoutAndGoToLogin()
                        }
                    #else
                        self.appDelegate.logoutAndGoToLogin()
                    #endif
                }
            }
        }
    }
    
    func webView(_ webView: WKWebView, didFail navigation: WKNavigation!, withError error: Error) {
        unlockView()
        helpLoaded = false
        self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
    }
    
    func lockView()
    {
        loader.startLoading(controller: self)
        self.tabBarController!.tabBar.isUserInteractionEnabled = false
        webView.scrollView.isScrollEnabled = false
    }
    
    
    func unlockView()
    {
        loader.stopLoading()
        self.tabBarController!.tabBar.isUserInteractionEnabled = true
        webView.scrollView.isScrollEnabled = true
    }
    
    func refreshTokensByJSInjection(accessToken : String, refreshToken : String) {
        if webView != nil {
            webView.evaluateJavaScript("matematici.tokenManager.setTokens(\"\(accessToken)\",\"\(refreshToken)\")")
        }
    }
    
}
