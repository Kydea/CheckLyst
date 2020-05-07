//
//  InstanceViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 22/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//


import UIKit
import WebKit

class InstanceViewController: UIViewController, WKNavigationDelegate, WKUIDelegate, WKScriptMessageHandler, UIGestureRecognizerDelegate, UIDocumentInteractionControllerDelegate {
    
    let app = UIApplication.shared as! MyApplication
    var instance:Instance!
    var instanceCodeFromRequest : String?
    var newInstance = false
    var newInstancePrototype: String?
    var newInstanceVersion: String?
    var selectedItemsDictionary = [Int:Option] ()
    var contentView : UIView?
    var scanButtonIsVisible : Bool = false
    var requestForScanning : Bool = false
    var requestForDocument : Bool = false
    let documentInteractionController = UIDocumentInteractionController()
    let supportedProtocols : Set = [ "http", "https", "mailto" ]
    var isInstanceClosed : Bool = false
    var scanButton : UIBarButtonItem?
    
    
    @IBOutlet weak var webView: WKWebView!
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    @IBOutlet var customBackButton: UIBarButtonItem!
    
    
    override func viewDidLoad()
    {
        super.viewDidLoad()
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        documentInteractionController.delegate = self
        webView.navigationDelegate = self
        webView.uiDelegate = self
        webView.configuration.userContentController.add(self, name: "instances")
        activityIndicator.hidesWhenStopped = true
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        self.definesPresentationContext = true
        
        scanButtonIsVisible = true
        
        for subview in webView.scrollView.subviews {
            if subview.classForCoder.description() == "WKContentView" {
                contentView = subview
            }
        }
        
        let fullUrl = !newInstance ?
            ServerConnection.baseFrontEndUrl + "/Instances/Detail/" + (instanceCodeFromRequest != nil ? instanceCodeFromRequest! : instance.code) + "?d=" + String(Int64(Date().timeIntervalSince1970 * 1000)) + "&AppUser=" + (UserDefaults.standard.string(forKey: "userCode"))! :
            ServerConnection.baseFrontEndUrl + "/Instances/Create/" + newInstancePrototype! + "/" + newInstanceVersion! + "?d=" + String(Int64(Date().timeIntervalSince1970 * 1000)) + "&AppUser=" + (UserDefaults.standard.string(forKey: "userCode"))!
        
        
        //print(fullUrl)
        
        let url = URL(string: fullUrl)
        
        let request = NSMutableURLRequest(url: url!)
        request.httpMethod = "POST"
        request.timeoutInterval = 20
        request.addValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")
        request.addValue((UserDefaults.standard.value(forKey: "token_type") as! String).capitalized + " " + (UserDefaults.standard.value(forKey: "access_token") as! String), forHTTPHeaderField: "Authorization")
        
        /*Setting value for local storage*/
        let bodyString = "roles=" + (UserDefaults.standard.value(forKey: "roles") as! String) +
            "&timeZone=" + (UserDefaults.standard.value(forKey: "timeZone") as! String) +
            "&decimalSeparator=" + (UserDefaults.standard.value(forKey: "decimalSeparator") as! String) + "&accessToken=" + (UserDefaults.standard.value(forKey: "access_token") as! String) + "&refreshToken=" + (UserDefaults.standard.value(forKey: "refresh_token") as! String)
        
        request.httpBody = bodyString.data(using: String.Encoding.utf8)
        
       //For testing -> gives 10 seconds to open safari inspector
       /* DispatchQueue.main.asyncAfter(deadline: .now() + .seconds(10)) {
            self.webView.load(request as URLRequest)
        }*/
        
        webView.load(request as URLRequest)
        
        self.lockView()
        NotificationCenter.default.addObserver(self, selector: #selector(InstanceViewController.keyboardWillShow(_:)), name: NSNotification.Name.UIKeyboardWillShow, object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(InstanceViewController.keyboardWillHide(_:)), name: NSNotification.Name.UIKeyboardWillHide, object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(InstanceViewController.keyboardDidHide(_:)), name: NSNotification.Name.UIKeyboardDidHide, object: nil)
        
        let tap: UITapGestureRecognizer = UITapGestureRecognizer(target: self, action: #selector(InstanceViewController.dismissKeyboard))
        tap.delegate = self
        self.view.addGestureRecognizer(tap)
        
        
        scanButton = UIBarButtonItem(barButtonSystemItem: .camera, target: self, action: #selector(scanItem(_:)))
        
        self.contentView!.inputAssistantItem.trailingBarButtonGroups = [UIBarButtonItemGroup(barButtonItems: [scanButton!], representativeItem: nil)] as! [UIBarButtonItemGroup]
    }
    
    override func didReceiveMemoryWarning()
    {
        super.didReceiveMemoryWarning()
    }
    
    func gestureRecognizer(_: UIGestureRecognizer,  shouldRecognizeSimultaneouslyWith shouldRecognizeSimultaneouslyWithGestureRecognizer:UIGestureRecognizer) -> Bool
    {
        return true
    }
    
    func lockView()
    {
        activityIndicator.startAnimating()
        self.webView.isHidden = true
        webView.scrollView.isScrollEnabled = false
        self.navigationItem.setHidesBackButton(true, animated:false)
        self.navigationItem.leftBarButtonItem = nil;
        self.tabBarController?.tabBar.isUserInteractionEnabled = false
    }
    
    func unlockView()
    {
        activityIndicator.stopAnimating()
        self.webView.isHidden = false
        webView.scrollView.isScrollEnabled = true
        self.navigationItem.setHidesBackButton(false, animated:false)
        self.navigationItem.leftBarButtonItem = self.customBackButton;
        self.tabBarController?.tabBar.isUserInteractionEnabled = true
    }
    
    override func viewDidAppear(_ animated: Bool) {
        super.viewDidAppear(animated)
        requestForScanning = false
        requestForDocument = false
        isInstanceClosed = false
    }
    
    override func viewWillDisappear(_ animated : Bool) {
        super.viewWillDisappear(animated)
        
        if(self.presentedViewController is QRCodeScannerViewController) {
            self.presentedViewController?.dismiss(animated: false, completion: nil)
            requestForScanning = false
        }

    }
    
    func closeInstance() {
        self.webView.evaluateJavaScript("instances.instance.close(function(){})", completionHandler: nil)
        isInstanceClosed = true
    }
    
    
    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        if message.name == "instances", let messageBody = message.body as? String {
            if messageBody.hasPrefix("quit") {
                self.app.goBackToList()
            } else if messageBody.hasPrefix("file") {
                activityIndicator.startAnimating()
                let fileURL = String(messageBody.split(separator: " ")[1])
                //print(fileURL)
                FileManager.default.getFile(fileURL) { (url, error) -> Void in
                    self.activityIndicator.stopAnimating()
                    if url != nil {
                        self.requestForDocument = true
                        self.documentInteractionController.url = url!
                        self.documentInteractionController.uti = url!.typeIdentifier ?? "public.data, public.content"
                        self.documentInteractionController.name = url!.localizedName ?? url!.lastPathComponent
                        self.documentInteractionController.presentPreview(animated: true)
                    } else if error != nil {
                        let alert = UIAlertController(title: "Error", message: "Connection error to the server", preferredStyle: .alert)
                        alert.addAction(UIAlertAction(title: "Ok", style: .destructive, handler: nil))
                        self.present(alert, animated: false, completion: nil)
                    } else {
                        let alert = UIAlertController(title: "Error", message: "Unable to retrieve document", preferredStyle: .alert)
                        alert.addAction(UIAlertAction(title: "Ok", style: .destructive, handler: nil))
                        self.present(alert, animated: false, completion: nil)
                    }
                }
            }
        }
    }
    
    override func viewDidLayoutSubviews() {
        
        webView.frame = CGRect(x: 0, y: 10, width: self.view.frame.size.width, height: self.view.frame.size.height)
        
    }
    
    func webView(_ webView: WKWebView, didFail navigation: WKNavigation!, withError error: Error) {
        unlockView()
        let alert = UIAlertController(title: "Error", message: "Connection error to the server", preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "Ok", style: .default, handler: { (action) -> Void in
            self.navigationController?.popViewController(animated: true)
        }))
        self.present(alert, animated: false, completion: nil)
    }
    
    func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
        unlockView()
        
        self.webView.isHidden = false
        
        webView.evaluateJavaScript("window.location.href") {
            (response, error) -> Void in
            if let value = response as? String {
                if(value.contains("Security/Login")) {
                    webView.isHidden = true
                    //print("redirecting")
                    self.app.logoutAndGoToLogin()
                }
            }
        }
    }
    
    func webView(_ webView: WKWebView, createWebViewWith configuration: WKWebViewConfiguration, for navigationAction: WKNavigationAction, windowFeatures: WKWindowFeatures) -> WKWebView? {
        if navigationAction.targetFrame == nil {
            if let scheme = navigationAction.request.url?.scheme {
                if supportedProtocols.contains(scheme) {
                    UIApplication.shared.open(navigationAction.request.url!, options: [:])
                } else {
                    let alert = UIAlertController(title: "Error", message: "Protocol \(scheme) not supported", preferredStyle: .alert)
                    alert.addAction(UIAlertAction(title: "Ok", style: .default, handler: nil))
                    self.present(alert, animated: false, completion: nil)
                }
            } else {
                let alert = UIAlertController(title: "Error", message: "Missing protocol", preferredStyle: .alert)
                alert.addAction(UIAlertAction(title: "Ok", style: .default, handler: nil))
                self.present(alert, animated: false, completion: nil)
            }
        }
        return nil
    }
    
    func shouldShowScanButton() {
        webView.evaluateJavaScript("document.activeElement.classList.contains(\"Scan\")") {
            (response, error) -> Void in
            if let value = response as? Bool {
                if(value) {
                    if (self.scanButton != nil && self.contentView != nil && !self.scanButtonIsVisible) {
                        self.scanButton?.isEnabled = true;
                        self.scanButtonIsVisible = true;
                    }
                } else if (self.contentView != nil && self.scanButtonIsVisible) {
                        self.scanButton?.isEnabled = false;
                        self.scanButtonIsVisible = false
                        //print("scan button off")
                }
            }
        }
    }
    
    func refreshTokensByJSInjection(accessToken : String, refreshToken : String) {
        webView.evaluateJavaScript("matematici.tokenManager.setTokens(\"\(accessToken)\",\"\(refreshToken)\")") /*{
            (response, error) -> Void in
            if let res = response {
                print(res)
            } else if let err = error {
                print(err)
            } else {
                print("Unexpected situation")
            }
        }*/
    }
    
    func setValueForInputField(_ value : String) {
        webView.evaluateJavaScript("$(document.activeElement).val(\"\(value)\").change(); ") {
            (response, error) -> Void in
            self.view.endEditing(true)
            self.requestForScanning = false
        }
        webView.evaluateJavaScript("document.activeElement")
    }
    
    @objc private func scanItem(_ sender: UIBarButtonItem) {
        //print("camera pressed")
        requestForScanning = true
        performSegue(withIdentifier: "ScanSegue", sender: self)
    }
    
    
    @IBAction func goBackToSearchInstances(_ sender: Any) {
        self.closeInstance()
        self.performSegue(withIdentifier: "goBackSearchInstances", sender: self)
    }
    
    @objc func keyboardWillShow(_ notification: Notification) {
        if view.frame.origin.y == 0{
            self.view.frame.origin.y -= 20
        }
        shouldShowScanButton()
    }
    
    
    @objc func keyboardWillHide(_ notification: Notification) {
        if view.frame.origin.y != 0 {
            self.view.frame.origin.y += 20
        }
    }
    
    @objc func keyboardDidHide(_ notification: Notification) {
        if (self.contentView != nil && scanButtonIsVisible) {
            self.scanButton?.isEnabled = false;
            scanButtonIsVisible = false
        }
    }
    
    @objc func dismissKeyboard() {
        self.view.endEditing(true)
        
    }
    
    func documentInteractionControllerViewControllerForPreview(_ controller: UIDocumentInteractionController) -> UIViewController {
        guard let navVC = self.navigationController else {
            return self
        }
        return navVC
    }
    
    func documentInteractionControllerWillBeginPreview(_ controller: UIDocumentInteractionController) {
        self.tabBarController?.tabBar.isHidden = true
    }
    
    func documentInteractionControllerDidEndPreview(_ controller: UIDocumentInteractionController) {
        requestForDocument = false
        self.tabBarController?.tabBar.isHidden = false
        FileManager.default.clearTemporaryDirectory()
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        if let controller = segue.destination as? QRCodeScannerViewController {
            controller.callback = self.setValueForInputField
            controller.senderController = self
        }
    }
}

