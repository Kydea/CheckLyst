//
//  ViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 21/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import UIKit

class LoginViewController: UIViewController, UITextFieldDelegate
    
    
{
    
    @IBOutlet var accessoryButton: UIButton!
    
    @IBOutlet var serverTextField: UITextField!
    
    @IBOutlet var userCodeTextField: UITextField!
    
    @IBOutlet var companyTextField: UITextField!
    
    @IBOutlet var passwordTextField: UITextField!
    
    @IBOutlet var ChooseServerButton: UIButton!
    
    @IBOutlet var EditServerButton: UIButton!
    
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    
    @IBOutlet var companyView: UIView!
    
    @IBOutlet var userCodeView: UIView!
    
    @IBOutlet var passwordView: UIView!
    
    @IBOutlet var loginButton: UIButton!
    
    @IBOutlet var verifyServerButton: UIButton!
    
    var isEditingServer = false
    
    var companyChooseDictionary : NSMutableDictionary = [:]
    
    fileprivate var _typedServerUrl : String?
    
    var selectedServerUrl : String {
        set(newValue) {
            _typedServerUrl = newValue
        }
        get {
            return _typedServerUrl ?? ServerConnection.baseUrl
            
        }
    }
    
    fileprivate var _typedServerFrontEndUrl : String?
    
    var selectedServerFrontEndUrl : String {
        set(newValue) {
            _typedServerFrontEndUrl = newValue
        }
        get {
            return _typedServerFrontEndUrl ?? ServerConnection.baseFrontEndUrl
            
        }
    }
    
    var selectedCompany : Company?
    
    let context = ((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)
    
    let appDelegate = UIApplication.shared as! MyApplication
    
    override func viewDidLoad() {
        
        super.viewDidLoad()
        
        serverTextField.delegate = self
        userCodeTextField.delegate = self
        passwordTextField.delegate = self
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
        
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.keyboardWillShow(_:)), name: .UIKeyboardWillShow, object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.keyboardWillHide(_:)), name: .UIKeyboardWillHide, object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.newServerConfiguration(_:)), name: .NewServerConfiguration, object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.serverHasBeenConfigured(_:)), name: .ServerHasBeenConfigured, object: nil)
        
        let tap: UITapGestureRecognizer = UITapGestureRecognizer(target: self, action: #selector(LoginViewController.dismissKeyboard))
        view.addGestureRecognizer(tap)
        
        activityIndicator.hidesWhenStopped = true
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        
    }
    
    @IBAction func verifyServer(_ sender: Any) {
        
        if (serverTextField.text!.isEmpty) {
            self.appDelegate.displayMessage(Title: "Error", Message: "Type a server address") { () -> Void in
                self.serverTextField.becomeFirstResponder()
            }
        } else {
            
            activityIndicator.startAnimating()
            self.view.isUserInteractionEnabled = false
            
            ServerConnection.getApiServerRequest(server: serverTextField.text!) { (serverResponse) -> Void in
                
                if (serverResponse.error != nil) {
                    
                    self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server \(self.serverTextField.text!) .")
                
                } else if (serverResponse.data != nil) {
                   
                    do {
                        
                        let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                        self._typedServerFrontEndUrl = self.serverTextField.text!
                        self._typedServerUrl = (json["API"] as! String)
                        self.toggleLogin(true, withAnimation: true)
                        
                    } catch {
                        
                        self.appDelegate.displayMessage(Title: "Error", Message: "Cannot parse response")
                        print("Error deserializing JSON: \(error)")
                        self.companyTextField.text = ""
                        self.selectedCompany = nil
                    
                    }
                
                }
                
                self.activityIndicator.stopAnimating()
                self.view.isUserInteractionEnabled = true
            
            }
        
        }
    
    }
    
    @IBAction func chooseServer(_ sender: Any) {
        
        performSegue(withIdentifier: "ChooseServer", sender: "server")
    
    }
    
    @IBAction func editServer(_ sender: Any) {
        
        self.appDelegate.displayMessage(Title: "Server Configuration", Message: "You are editing the server address.\nAre you sure?", canDecline: true) { () -> Void in
            
            self.toggleLogin(false, withAnimation: true)
            self.prepareServerForNewConfiguration()
        
        }
        
    }
    
    @IBAction func chooseCompany(_ sender: Any) {
        
        activityIndicator.startAnimating()
        
        self.view.isUserInteractionEnabled = false
        
        ServerConnection.getCompaniesRequest(server: selectedServerUrl){ (serverResponse) -> Void in
            
            if(serverResponse.error != nil) {
                
                self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
            }
            
            do{
                
                if serverResponse.data != nil
                {
                    let json = try JSONDecoder().decode([Company].self, from: serverResponse.data!)
                    
                    self.companyChooseDictionary.removeAllObjects()
                    
                    for company in json {
                        self.companyChooseDictionary.addEntries(from: [company.name : ["id" : String(company.id), "inactivityTimeoutSeconds" : String(company.inactivityTimeoutSeconds)]])
                    }
                    
                    self.activityIndicator.stopAnimating()
                    
                    self.view.isUserInteractionEnabled = true
                    
                    self.performSegue(withIdentifier: "ChooseServer", sender: "company")
                    
                }
                
            } catch {
                self.appDelegate.displayMessage(Title: "Error", Message: "Cannot parse response")
                print("Error deserializing JSON: \(error)")
            }
            
            self.activityIndicator.stopAnimating()
            
            self.view.isUserInteractionEnabled = true
            
        }
    }
    
    
    override func didReceiveMemoryWarning()
        
    {
        
        super.didReceiveMemoryWarning()
        
    }
    
    override func viewWillAppear(_ animated: Bool) {
        
        super.viewWillAppear(animated)
        let showServer = Server.isSetServer(managedObjectContext: self.context) || _typedServerFrontEndUrl != nil
        if(showServer) {
            self.serverTextField.text = selectedServerFrontEndUrl
        }
        self.toggleLogin(showServer, withAnimation: false)
        
    }
    
    
    func textFieldShouldReturn(_ textField: UITextField) -> Bool {
        
        textField.resignFirstResponder()
        
        return true
    }
    
    
    
    @objc func keyboardWillShow(_ notification: Notification) {
        
        if view.frame.origin.y == 0{
            self.view.frame.origin.y -= 150
        }
        
    }
    
    
    @objc func keyboardWillHide(_ notification: Notification) {
        
        if view.frame.origin.y != 0 {
            self.view.frame.origin.y += 150
        }
        
    }
    
    @IBAction func loginToserver(_ sender: UIButton) {
        
        if(companyTextField.text! != "" && userCodeTextField.text! != "" && passwordTextField.text! != "") {
            
            if(serverTextField.text!.hasPrefix("https")) {
                
                performLogin()
            
            } else {
                
                self.appDelegate.displayMessage(Title: "Warning", Message: "You are connecting to an insecure server, are you really sure?", canDecline: true) { () -> Void in
                    self.performLogin()
                }
            
            }
        } else if(companyTextField.text! == "" ) {
            
            self.appDelegate.displayMessage(Title: "Error", Message: "Select a company")
            
        } else if(userCodeTextField.text! == "") {
            
            self.appDelegate.displayMessage(Title: "Error", Message: "Select a user code")
            
        } else if(passwordTextField.text! == "") {
            
            self.appDelegate.displayMessage(Title: "Error", Message: "Select a password")
            
        }
        
    }
    
    
    
    func performLogin() {
        
        activityIndicator.startAnimating()
        
        self.view.isUserInteractionEnabled = false
        
        ServerConnection.loginOrRefreshToServer(server: selectedServerUrl, ["Company" : String(self.selectedCompany!.id),"Code":userCodeTextField.text!,"Password":passwordTextField.text!]){ (serverResponse) -> Void in
            
            if(serverResponse.error != nil) {
                
                var errorMessage = "Connection error to the server"
                var title = "Error"
                
                if(serverResponse.error!.code == -1009) {
                    errorMessage = "Connection error to the server"
                } else if(NSString(string:UserDefaults.standard.string(forKey: "invalidCertificate")!).boolValue == true) {
                    
                    title = "Security Error"
                    errorMessage = "Invalid SSL Certificate"
                
                }
                
                self.appDelegate.displayMessage(Title: title, Message: errorMessage)
            }
            
            do{
                
                if serverResponse.data != nil {
                    
                    let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                    
                    if(json["error"] == nil) {
                        
                        Server.addServerUrl(urlFrontEnd: self.selectedServerFrontEndUrl, managedObjectContext: self.context)
                        Server.setNewServer(urlFrontEnd: self.selectedServerFrontEndUrl, apiUrl: self.selectedServerUrl, managedObjectContext: self.context)
                        
                        self.setUserInformation(userInfo: json)
                        
                        (UIApplication.shared as! MyApplication).refreshInactivityTimer(timeToExpireActivity: (UIApplication.shared as! MyApplication).inactivityInterval)
                        
                        self.getClient()
                        
                    } else {
                        
                        let okMex = (json["error"] == nil)
                        let messageMex = !okMex ? json["error_description"] as! String : ""
                        self.appDelegate.displayMessage(Title: "Error", Message: messageMex)
                    
                    }
                    
                }
            
            } catch {
                self.appDelegate.displayMessage(Title: "Error", Message: "Cannot parse response")
                print("Error deserializing JSON: \(error)")
            }
            
            self.activityIndicator.stopAnimating()
            
            self.view.isUserInteractionEnabled = true
        }
        
    }
    
    func getClient() {
        
        ServerConnection.sendGetClientRequest(["data":["code":UserDefaults.standard.value(forKey: "userCode") as! String, "description":""]]){ (serverResponse) -> Void in
            
            if(serverResponse.data != nil && serverResponse.error == nil) {
                
                self.performSegue(withIdentifier: "loggedSegue", sender: self)
            
            } else {
               
                self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
            
            }
            
        }
        
    }
    
    func setUserInformation(userInfo json:[String:Any]) {
        
        UserDefaults.standard.setValue(json["code"], forKey: "userCode")
        UserDefaults.standard.setValue(true, forKey: "logged")
        UserDefaults.standard.setValue(json["access_token"], forKey: "access_token")
        UserDefaults.standard.setValue(json["refresh_token"], forKey: "refresh_token")
        UserDefaults.standard.setValue(json["token_type"], forKey: "token_type")
        UserDefaults.standard.setValue(json["roles"], forKey: "roles")
        UserDefaults.standard.setValue((json["expires_in"] as! Int).description, forKey: "expires_in")
        UserDefaults.standard.setValue(json["company"], forKey: "company")
        UserDefaults.standard.setValue(json["name"], forKey: "name")
        UserDefaults.standard.setValue(json["timeZone"], forKey: "timeZone")
        UserDefaults.standard.setValue(self.companyTextField.text!, forKey: "companyName")
        UserDefaults.standard.setValue(self.selectedCompany!.inactivityTimeoutSeconds, forKey: "inactivityTimeoutSeconds")
        UserDefaults.standard.setValue(Date() + (json["expires_in"] as! TimeInterval), forKey: "token_expires")
        UserDefaults.standard.setValue(json["decimalSeparator"], forKey: "decimalSeparator")
        
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        
        if let controller = segue.destination as? ChooseServerAndCompanyViewController {
            
            if(sender as! String == "server") {
                    
                let serverChooseDictionary = Server.getServers(context)
                controller.serverFieldsDictionary = serverChooseDictionary
                
                controller.fieldNameString = "Select Server"
                
            } else {
                controller.companyFieldsDictionary = companyChooseDictionary as? Dictionary<String,Dictionary<String, String>>
                
                controller.fieldNameString = "Select Company"
            }
                
            controller.loginViewController = self
                
            controller.chooseServer = (sender as! String) == "server" ? true : false
                
        } else {
           
            view.endEditing(true)
            passwordTextField.text = ""
            userCodeTextField.text = ""
            _typedServerFrontEndUrl = nil
            _typedServerUrl = nil
        
        }
        
    }
    
    
    func textFieldDidEndEditing(_ textField: UITextField) {
        
        if(textField.isEqual(serverTextField)) {
            isEditingServer = false
        }
    
    }
    
    func textFieldDidBeginEditing(_ textField: UITextField) {
        
        if(textField.isEqual(serverTextField)){
            isEditingServer = true
        }
        
    }
    
    func toggleLogin(_ value:Bool, withAnimation animation:Bool) {
        
        if(animation) {
            
            UIView.animate(withDuration: 0.3){
                
                self.passwordView.isHidden = !value
                self.userCodeView.isHidden = !value
                self.companyView.isHidden = !value
            
            }
            
            self.loginButton.isHidden = !value
            self.verifyServerButton.isHidden = value
            self.serverTextField.isEnabled = !value
            self.ChooseServerButton.isHidden = Server.any(managedObjectContext: self.context) ? value : true
            self.EditServerButton.isHidden = !value
        
        } else {
            
            self.companyView.isHidden = !value
            self.userCodeView.isHidden = !value
            self.passwordView.isHidden = !value
            self.loginButton.isHidden = !value
            self.verifyServerButton.isHidden = value
            self.serverTextField.isEnabled = !value
            self.ChooseServerButton.isHidden = Server.any(managedObjectContext: self.context) ? value : true
            self.EditServerButton.isHidden = !value
            
        }
        
    }
    
    @objc func dismissKeyboard() {
        
        view.endEditing(true)
        
    }
    
    func prepareServerForNewConfiguration() {
        
        self.selectedCompany = nil
        self.companyTextField.text = ""
        self.serverTextField.text = ""
        self._typedServerFrontEndUrl = nil
        Server.unsetCurrentServer(managedObjectContext: self.context)
        
    }
    
    @objc func newServerConfiguration(_ notification:Notification) {

        self.activityIndicator.startAnimating()
        prepareServerForNewConfiguration()

    }
    
    @objc func serverHasBeenConfigured(_ notification:Notification) {

        self.activityIndicator.stopAnimating()
        self.viewWillAppear(true)

    }
    
}



