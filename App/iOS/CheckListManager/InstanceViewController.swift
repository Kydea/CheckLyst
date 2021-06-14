//
//  InstanceViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 22/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//


import UIKit
import WebKit

class InstanceViewController: UIViewController, WKNavigationDelegate, WKUIDelegate, WKScriptMessageHandler, UIGestureRecognizerDelegate, UIDocumentInteractionControllerDelegate, UIImagePickerControllerDelegate, UINavigationControllerDelegate {
    
    let appDelegate = UIApplication.shared as! MyApplication
    var instance:Instance!
    var instanceCodeFromRequest : String?
    var newInstance = false
    var newInstancePrototype: String?
    var newInstanceVersion: String?
    var newInstanceFieldsValues: String?
    var selectedItemsDictionary = [Int:Option] ()
    var contentView : UIView?
    var scanButtonIsVisible : Bool = false
    var requestForScanning : Bool = false
    var requestForDocument : Bool = false
    let documentInteractionController = UIDocumentInteractionController()
    let supportedProtocols : Set = [ "http", "https", "mailto" ]
    var isInstanceClosed : Bool = false
    var scanButton : UIBarButtonItem?
    var creationTime: Date?
    var currentAccessToken: String?
    var oldImagePickerDelegate: UIImagePickerControllerDelegate?
    var imagePickerController: UIImagePickerController = UIImagePickerController()
    
    
    @IBOutlet weak var webView: WKWebView!
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    @IBOutlet var customBackButton: UIBarButtonItem!
    
    
    override func viewDidLoad()
    {
        super.viewDidLoad()
        creationTime = Date()
        currentAccessToken = (UserDefaults.standard.value(forKey: "access_token") as! String)
        self.appDelegate.currentWebViewCreationTime = creationTime
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
            ServerConnection.baseFrontEndUrl + "/Instances/Create/" + newInstancePrototype! + "/" + newInstanceVersion! + "?" + (newInstanceFieldsValues! != "" ? newInstanceFieldsValues! + "&" : "") + "d=" + String(Int64(Date().timeIntervalSince1970 * 1000)) + "&AppUser=" + (UserDefaults.standard.string(forKey: "userCode"))!
        
                
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
        NotificationCenter.default.addObserver(self, selector: #selector(InstanceViewController.signersHaveBeenChanged(_:)), name: .SignersHaveBeenChanged, object: nil)
        let tap: UITapGestureRecognizer = UITapGestureRecognizer(target: self, action: #selector(InstanceViewController.dismissKeyboard))
        tap.delegate = self
        self.view.addGestureRecognizer(tap)
        
        scanButton = UIBarButtonItem(barButtonSystemItem: .camera, target: self, action: #selector(scanItem(_:)))
        
        self.contentView!.inputAssistantItem.trailingBarButtonGroups = [UIBarButtonItemGroup(barButtonItems: [scanButton!], representativeItem: nil)] as! [UIBarButtonItemGroup]
        
        FileManager.default.clearTemporaryDirectory()
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
        
        if let imagePicker = self.presentedViewController as? UIImagePickerController, !imagePicker.isKind(of: QRCodeScannerViewController.self), imagePicker.sourceType == .camera {
            setupWatermarkImagePicker(imagePicker)
        }

    }
    
    private func setupWatermarkImagePicker(_ controller: UIImagePickerController) {
            oldImagePickerDelegate = controller.delegate
            imagePickerController = controller
            imagePickerController.delegate = self
    }
        
    func imagePickerControllerDidCancel(_ picker: UIImagePickerController) {
        picker.delegate = oldImagePickerDelegate as? UIImagePickerControllerDelegate & UINavigationControllerDelegate
        oldImagePickerDelegate?.imagePickerControllerDidCancel!(picker)
    }
    
    func imagePickerController(_ picker: UIImagePickerController, didFinishPickingMediaWithInfo info: [String : Any]) {
        var newInfo = info
        if let pickedImage = newInfo[UIImagePickerControllerOriginalImage] as? UIImage {
            
            let imageWithWatermark = pickedImage.addWatermark(position: .RightBottom, watermark: WatermarkGenerator.getWatermark(size: CGSize(width: 400, height: 100)) ?? UIImage(), margin: 10)
            
            newInfo[UIImagePickerControllerOriginalImage] = imageWithWatermark
        }

        picker.delegate = oldImagePickerDelegate as? UIImagePickerControllerDelegate & UINavigationControllerDelegate
        oldImagePickerDelegate?.imagePickerController!(picker, didFinishPickingMediaWithInfo: newInfo)
    }
    
    func closeInstance() {
        self.webView.evaluateJavaScript("instances.instance.close(function(){})")
        self.cleanWebViewController()
        isInstanceClosed = true
    }
    
    
    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        if message.name == "instances", let messageBody = message.body as? String {
            if messageBody.hasPrefix("quit") {
                self.cleanWebViewController()
                self.appDelegate.goBackToList()
            } else if messageBody.hasPrefix("file") {
                activityIndicator.startAnimating()
                let fileURL = String(messageBody.split(separator: " ")[1])
                //print(fileURL)
                    FileManager.default.downloadFile(fileURL) { (url, error, data) -> Void in
                    self.activityIndicator.stopAnimating()
                    if url != nil {
                        if self.checkFile(file: data, url: fileURL) {
                            self.requestForDocument = true
                            self.documentInteractionController.url = url!
                            self.documentInteractionController.uti = url!.typeIdentifier ?? "public.data, public.content"
                            self.documentInteractionController.name = url!.localizedName ?? url!.lastPathComponent
                            self.documentInteractionController.presentPreview(animated: true)
                        }
                    } else if error != nil {
                        self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
                    } else {
                        self.appDelegate.displayMessage(Title: "Error", Message: "Unable to retrieve document")
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
        self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server") { () -> Void in
            self.navigationController?.popViewController(animated: true)
        }
    }
    
    func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
        unlockView()
        
        self.webView.isHidden = false
        
        webView.evaluateJavaScript("window.location.href") {
            (response, error) -> Void in
            if let value = response as? String {
                if(value.contains("Security/Login")) {
                    webView.isHidden = true
                    #if DEBUG
                        let message = String(format: "InstanceViewController\nCurrent WKWebView creationTime: %@\nLogout WKWebView creationTime: %@\nCurrent accessToken: %@\nLogout WKWebView accessToken: %@\n", Formatter.instance.stringFromTestDate( self.appDelegate.currentWebViewCreationTime!), Formatter.instance.stringFromTestDate(self.creationTime!), (UserDefaults.standard.value(forKey: "access_token") as? String) ?? "nil", self.currentAccessToken ?? "nil")
                        try? message.write(to: FileManager.default.getDocumentsDirectory().appendingPathComponent("error.log"), atomically: true, encoding: .utf8)
                        self.appDelegate.displayMessage(Title: "Security Error", Message: message) { [weak self] in
                            self?.appDelegate.logoutAndGoToLogin()
                        }
                    #else
                        self.appDelegate.logoutAndGoToLogin()
                    #endif
                } else {
                    self.setOrRefreshSigners(SignersManager.shared.signers)
                }
            }
        }
    }
    
    func webView(_ webView: WKWebView, decidePolicyFor
           navigationAction: WKNavigationAction,
           decisionHandler: @escaping (WKNavigationActionPolicy) -> Swift.Void) {

       if navigationAction.sourceFrame == nil {
            decisionHandler(.allow)
            return
        }
        
        guard let url = navigationAction.request.url, let scheme = url.scheme else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Missing protocol")
            decisionHandler(.cancel)
            return
        }
        
        if !supportedProtocols.contains(scheme) {
            self.appDelegate.displayMessage(Title: "Error", Message: "Protocol \(scheme) not supported")
            decisionHandler(.cancel)
            return
        }
                
        if url.schemeHost == ServerConnection.baseFrontEndUrl &&
        (url.getCallType == .Create ||
        url.getCallType == .Detail) {
            #if DEBUG
            print("Redirect to \(url.absoluteString)")
            #endif
            appDelegate.requestToOpenInstance = true
            appDelegate.requestedUrlInfo = url.getParametersForInstance
            self.goBackToSearchInstances(self)
            decisionHandler(.cancel)
            return
        }
        
        #if DEBUG
        print("Redirect to \(url.absoluteString)")
        #endif
        UIApplication.shared.open(navigationAction.request.url!, options: [:])
        decisionHandler(.allow)

    }
    
    func webView(_ webView: WKWebView, createWebViewWith configuration: WKWebViewConfiguration, for navigationAction: WKNavigationAction, windowFeatures: WKWindowFeatures) -> WKWebView? {
        if navigationAction.targetFrame == nil {
            if let url = navigationAction.request.url, let scheme = url.scheme {
                if supportedProtocols.contains(scheme) {
                    if url.schemeHost == ServerConnection.baseFrontEndUrl {
                        if (url.getCallType == .Create ||
                            url.getCallType == .Detail) {
                            #if DEBUG
                            print("Redirect to \(url.absoluteString)")
                            #endif
                            appDelegate.requestToOpenInstance = true
                            appDelegate.requestedUrlInfo = url.getParametersForInstance
                            self.goBackToSearchInstances(self)
                        }
                    } else {
                        #if DEBUG
                        print("Redirect to \(url.absoluteString)")
                        #endif
                        UIApplication.shared.open(navigationAction.request.url!, options: [:])
                    }
                } else {
                    self.appDelegate.displayMessage(Title: "Error", Message: "Protocol \(scheme) not supported")
                }
            } else {
                self.appDelegate.displayMessage(Title: "Error", Message: "Missing protocol")
            }
        }
        return nil
    }
    
    func shouldShowScanButton() {
        webView.evaluateJavaScript("document.activeElement.classList.contains(\"Scan\");") {
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
                }
            }
        }
    }
    
    func refreshTokensByJSInjection(accessToken : String, refreshToken : String) {
        webView.evaluateJavaScript("matematici.tokenManager.setTokens(\"\(accessToken)\",\"\(refreshToken)\")")
        self.currentAccessToken = accessToken
    }
    
    func setOrRefreshSigners(_ signers:[Signer]) {
        guard let signersJSON = try? JSONEncoder().encode(signers) else { return }
        let signersString = String(decoding: signersJSON, as: UTF8.self)
        print("matematici.tokenManager.setSigners(JSON.stringify(\(signersString)))")
        webView.evaluateJavaScript("matematici.tokenManager.setSigners(JSON.stringify(\(signersString)))")
    }
    
    @objc func signersHaveBeenChanged(_ notification:Notification) {
        setOrRefreshSigners(SignersManager.shared.signers)
        webView.reload()
    }
    
    func setValueForInputField(_ value : String) {
        webView.evaluateJavaScript("document.activeElement.value = \"\(value)\";document.activeElement.dispatchEvent(new Event(\"change\"));") {
            (response, error) -> Void in
            self.view.endEditing(true)
            self.requestForScanning = false
        }
    }
    
    func checkFile(file data : Data?, url : String) -> Bool {
        
        guard let file = data else {
            self.appDelegate.displayMessage(Title: "Error", Message: "Can't display file\nFile is empty")
            return false
        }
        
        if let json = try? JSONSerialization.jsonObject(with: file, options: .allowFragments) as! [String:Any], let messages = json["messages"] as? [String] {
            self.appDelegate.displayMessage(Title: "Error", Message: "Can't display file\n\(messages.joined(separator: "\n"))")
            return false
        }
        
        if let dataString = String(bytes: file, encoding: .utf8) {
            self.appDelegate.displayMessage(Title: "Error", Message: "Can't display file\n\(dataString)")
            return false
        }
        
        return true
    
    }
    
    @objc private func scanItem(_ sender: UIBarButtonItem) {
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
    
    func cleanWebViewController() {
        webView.configuration.userContentController.removeScriptMessageHandler(forName: "instances")
        webView.uiDelegate = nil
        webView.navigationDelegate = nil
        documentInteractionController.delegate = nil
        NotificationCenter.default.removeObserver(self)
        webView.load(URLRequest(url: URL(string: "about:blank")!))
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
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        if let controller = segue.destination as? QRCodeScannerViewController {
            controller.callback = self.setValueForInputField
            controller.senderController = self
        }
    }
}

