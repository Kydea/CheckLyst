//
//  InstanceViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 22/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//


import UIKit

class InstanceViewController: UIViewController, UIWebViewDelegate, UIGestureRecognizerDelegate {
    
    let app = UIApplication.shared as! MyApplication
    var instance:Instance!
    var newInstance = false
    var newInstancePrototype: String?
    var newInstanceVersion: String?
    var selectedItemsDictionary = [Int:Option] ()
    @IBOutlet var webView: UIWebView!
    @IBOutlet var activityIndicator: UIActivityIndicatorView!

    override func viewDidLoad()
    {
        super.viewDidLoad()
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        webView.delegate = self
        activityIndicator.hidesWhenStopped = true
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        
        let fullUrl = !newInstance ?
            "http://clm.matematici-aws.com/Instances/Detail/" + instance.code + "?d=" + String(Int64(Date().timeIntervalSince1970 * 1000)) + "&AppUser=" + (UserDefaults.standard.string(forKey: "userCode"))! :
            "http://clm.matematici-aws.com/Instances/Create/" + newInstancePrototype! + "/" + newInstanceVersion! + "?d=" + String(Int64(Date().timeIntervalSince1970 * 1000)) + "&AppUser=" + (UserDefaults.standard.string(forKey: "userCode"))!
        
        let url = URL(string: fullUrl)
        
        let request = NSMutableURLRequest(url: url!)
        request.httpMethod = "POST"
        request.timeoutInterval = 20
        request.addValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")
        /*Setting value for local storage*/
        let bodyString = "roles=" + (UserDefaults.standard.value(forKey: "roles") as! String) + "&timeZone=" + (UserDefaults.standard.value(forKey: "timeZone") as! String)
        print(bodyString)
        request.httpBody = bodyString.data(using: String.Encoding.utf8)
        
        webView.loadRequest(request as URLRequest)
        //webView.loadRequest(URLRequest(url:URL(string:"http://www.google.com")!))
        
        self.lockView()
        NotificationCenter.default.addObserver(self, selector: #selector(InstanceViewController.keyboardWillShow(_:)), name: NSNotification.Name.UIKeyboardWillShow, object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(InstanceViewController.keyboardWillHide(_:)), name: NSNotification.Name.UIKeyboardWillHide, object: nil)
        let tap: UITapGestureRecognizer = UITapGestureRecognizer(target: self, action: #selector(InstanceViewController.dismissKeyboard))
        tap.delegate = self
        self.view.addGestureRecognizer(tap)
    }
    
    override func didReceiveMemoryWarning()
    {
        super.didReceiveMemoryWarning()
    }
    
    func gestureRecognizer(_: UIGestureRecognizer,  shouldRecognizeSimultaneouslyWith shouldRecognizeSimultaneouslyWithGestureRecognizer:UIGestureRecognizer) -> Bool
    {
        return true
    }
    
    
    func willEnterForeground()
    {
        self.navigationController?.popViewController(animated: true)
    }

    
    func lockView()
    {
        activityIndicator.startAnimating()
        self.webView.isHidden = true
        webView.scrollView.isScrollEnabled = false
        self.navigationItem.setHidesBackButton(true, animated:false)
        self.tabBarController!.tabBar.isUserInteractionEnabled = false
    }
    
    func unlockView()
    {
        activityIndicator.stopAnimating()
        self.webView.isHidden = false
        webView.scrollView.isScrollEnabled = true
        self.navigationItem.setHidesBackButton(false, animated:false)
        self.tabBarController!.tabBar.isUserInteractionEnabled = true
    }

    
    override func viewWillDisappear(_ animated : Bool) {
        super.viewWillDisappear(animated)
        
    }

    
    override func viewDidLayoutSubviews() {
        
        webView.frame = CGRect(x: 0, y: 10, width: self.view.frame.size.width, height: self.view.frame.size.height)
        
    }

    
    
    func webView(_ webView: UIWebView, didFailLoadWithError error: Error) {
        
        unlockView()
        let alert = UIAlertController(title: "Error", message: "Connection error to the server", preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "Ok", style: .default, handler: { (action) -> Void in
            self.navigationController?.popViewController(animated: true)
        }))
        self.present(alert, animated: false, completion: nil)
    }
    
    func webView(_ webView: UIWebView, shouldStartLoadWith request: URLRequest, navigationType: UIWebViewNavigationType) -> Bool {

        return true
    }
    
    func webViewDidStartLoad(_ webView: UIWebView) {
       // print("Webview started Loading")
    }
    
    
    
    func webViewDidFinishLoad(_ webView: UIWebView) {
    
        unlockView()
//        if let request = self.webView.request {
//            if let resp = URLCache.shared.cachedResponse(for: request) {
//                if let response = resp.response as? HTTPURLResponse {
//                    print(response.allHeaderFields)
//                }
//            }
//        }
        self.webView.isHidden = false
        
        if( webView.stringByEvaluatingJavaScript(from: "window.location.href")!.contains("Security/Login")){
            print("redirecting")
            webView.isHidden = true
            app.logoutAndGoToLogin()
        }
        
    }

    @objc func keyboardWillShow(_ notification: Notification) {
        if view.frame.origin.y == 0{
            self.view.frame.origin.y -= 20
        }
    }
    
    
    @objc func keyboardWillHide(_ notification: Notification) {
        if view.frame.origin.y != 0 {
            self.view.frame.origin.y += 20
        }
    }
    
    @objc func dismissKeyboard() {
        self.view.endEditing(true)   }
}

