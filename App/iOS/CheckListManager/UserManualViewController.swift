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
    
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    
    let app = UIApplication.shared as! MyApplication
    
    var helpLoaded : Bool = false
    
    override func viewDidLoad()
        
    {
        super.viewDidLoad()
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
        
        webView.navigationDelegate = self
        
        activityIndicator.hidesWhenStopped = true
        
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        
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
                    self.app.logoutAndGoToLogin()
                }
            }
        }
    }
    
    func webView(_ webView: WKWebView, didFail navigation: WKNavigation!, withError error: Error) {
    unlockView()
        helpLoaded = false
        
        let alert = UIAlertController(title: "Error", message: "Connection error to the server", preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "Ok", style: .default, handler: { (action) -> Void in
        self.tabBarController?.selectedIndex = 0
        }))
        self.present(alert, animated: false, completion: nil)
    }
    
    func lockView()
    {
        activityIndicator.startAnimating()
        self.tabBarController!.tabBar.isUserInteractionEnabled = false
        webView.scrollView.isScrollEnabled = false
    }
    
    
    func unlockView()
    {
        activityIndicator.stopAnimating()
        self.tabBarController!.tabBar.isUserInteractionEnabled = true
        webView.scrollView.isScrollEnabled = true
    }
    
}
