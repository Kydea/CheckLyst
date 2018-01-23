//
//  UserManualViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 27/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

class UserManualViewController: UIViewController, UIWebViewDelegate {

    @IBOutlet var webView: UIWebView!
    
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    
    let app = UIApplication.shared as! MyApplication

    
    override func viewDidLoad()
    
    {
        super.viewDidLoad()
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
        
        webView.delegate = self
        
        activityIndicator.hidesWhenStopped = true
        
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        
        let fullUrl = "http://clm.matematici-aws.com/Help/App?AppMode=1&d=" + String(Int64(Date().timeIntervalSince1970 * 1000))
        
        let url = URL(string: fullUrl)
        
        let request = NSMutableURLRequest(url: url!)
        
        request.httpMethod = "GET"
        
        request.timeoutInterval = 10
        
        webView.loadRequest(request as URLRequest)
        
        self.lockView()

    }

    
    override func viewDidLayoutSubviews() {
        
        webView.frame = CGRect(x: 0, y: 10, width: self.view.frame.size.width, height: self.view.frame.size.height)
        
    }
    
    

    func webView(_ webView: UIWebView, shouldStartLoadWith request: URLRequest, navigationType: UIWebViewNavigationType) -> Bool {
        
        return true;
        
    }

    
    
    
    func webViewDidFinishLoad(_ webView: UIWebView) {
        
        unlockView()

        if( webView.stringByEvaluatingJavaScript(from: "window.location.href")!.contains("Security/Login")){
            
            webView.isHidden = true
            
            logoutAndGoToLogin()
        }
    }
    
    
    func webView(_ webView: UIWebView, didFailLoadWithError error: Error) {
        
        unlockView()
        
        let alert = UIAlertController(title: "Error", message: "No internet connection", preferredStyle: .alert)
        
        alert.addAction(UIAlertAction(title: "Ok", style: .default, handler: { (action) -> Void in
            
            self.tabBarController?.selectedIndex = 0
            
        }))
        
        self.present(alert, animated: false, completion: nil)
    }

    
    
    func logoutAndGoToLogin()
        
    {
        app.logoutAndGoToLogin()
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
