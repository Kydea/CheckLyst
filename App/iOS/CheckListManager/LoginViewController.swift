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
    
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    
    var isEditingServer = false
    
    var companyChooseDictionary : NSMutableDictionary = [:]
    
    fileprivate var _selectedServerUrl : String?
    
    var selectedServerUrl : String { return _selectedServerUrl ?? ServerConnection.baseUrl }
    
    fileprivate var _selectedServerFrontEndUrl : String?
    
    var selectedServerFrontEndUrl : String { return _selectedServerFrontEndUrl ?? ServerConnection.baseFrontEndUrl }
    
    var selectedCompany : Company?
    
    let context = ((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)
    
    
    override func viewDidLoad()
        
    {
        
        super.viewDidLoad()
        
        serverTextField.delegate = self
        
        userCodeTextField.delegate = self
        
        passwordTextField.delegate = self
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
        
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.keyboardWillShow(_:)), name: NSNotification.Name.UIKeyboardWillShow, object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.keyboardWillHide(_:)), name: NSNotification.Name.UIKeyboardWillHide, object: nil)
        
        let tap: UITapGestureRecognizer = UITapGestureRecognizer(target: self, action: #selector(LoginViewController.dismissKeyboard))
        
        view.addGestureRecognizer(tap)
        
        activityIndicator.hidesWhenStopped = true
        
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        
    }
    
    @IBAction func chooseServer(_ sender: Any) {
        performSegue(withIdentifier: "ChooseServer", sender: "server")
    }
    
    @IBAction func chooseCompany(_ sender: Any) {
        
        if (!self.isEditingServer) {
            activityIndicator.startAnimating()
            
            self.view.isUserInteractionEnabled = false
            
            ServerConnection.getCompaniesRequest(server: selectedServerUrl){ (serverResponse) -> Void in
                
                if(serverResponse.error != nil){
                    
                    let errorMessage = "Connection error to the server"
                    let title = "Error"
                    
                    
                    self.showSimpleErrorAlert(title, message: errorMessage)
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
                    print("Error deserializing JSON: \(error)")
                }
                
                self.activityIndicator.stopAnimating()
                
                self.view.isUserInteractionEnabled = true
                
            }
        } else {
            self.serverTextField.endEditing(true);
        }
    }
    
    
    override func didReceiveMemoryWarning()
        
    {
        
        super.didReceiveMemoryWarning()
        
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        let showServerHistory = Server.any(managedObjectContext: self.context)
        self.ChooseServerButton.isHidden = !showServerHistory
        self.serverTextField.text = showServerHistory ? ServerConnection.baseFrontEndUrl : ""
    }
    
    
    func textFieldShouldReturn(_ textField: UITextField) -> Bool {
        
        
        textField.resignFirstResponder()
        
        return true
    }
    
    
    
    @objc func keyboardWillShow(_ notification: Notification) {
        
        
        if view.frame.origin.y == 0{
            self.view.frame.origin.y -= 190
        }
        
        
        
    }
    
    
    @objc func keyboardWillHide(_ notification: Notification) {
        
        if view.frame.origin.y != 0 {
            self.view.frame.origin.y += 190
        }
        
    }
    
    @IBAction func loginToserver(_ sender: UIButton) {
        
        if(companyTextField.text! != "" && userCodeTextField.text! != "" && passwordTextField.text! != "") {
            
            if(serverTextField.text!.hasPrefix("https"))
                
            {
                performLogin()
            }
                
                
            else
                
            {
                
                let alert = UIAlertController(title: "Warning", message: "You are connecting to an insecure server, are you really sure?", preferredStyle: .alert)
                
                
                
                alert.addAction(UIAlertAction(title: "No", style: .cancel, handler: nil))
                
                alert.addAction(UIAlertAction(title: "Yes", style: .default, handler: { (action) -> Void in
                    
                    self.performLogin()
                    
                }))
                
                
                self.present(alert, animated: true, completion: nil)
                
            }
        } else if(companyTextField.text! == "" ) {
            
            self.showSimpleErrorAlert("Error", message: "Select a company")
            
        } else if(userCodeTextField.text! == "") {
            
            self.showSimpleErrorAlert("Error", message: "Select a user code")
            
        } else if(passwordTextField.text! == "") {
            
            self.showSimpleErrorAlert("Error", message: "Select a password")
            
        }
        
        
    }
    
    
    
    func performLogin()
        
    {
        activityIndicator.startAnimating()
        
        self.view.isUserInteractionEnabled = false
        
        ServerConnection.loginOrRefreshToServer(server: selectedServerUrl, ["Company" : String(self.selectedCompany!.id),"Code":userCodeTextField.text!,"Password":passwordTextField.text!]){ (serverResponse) -> Void in
            
            if(serverResponse.error != nil) {
                
                var errorMessage = "Connection error to the server"
                var title = "Error"
                
                if(serverResponse.error!.code == -1009){
                    errorMessage = "Connection error to the server"
                }
                    
                else if(NSString(string:UserDefaults.standard.string(forKey: "invalidCertificate")!).boolValue == true)
                    
                {
                    title = "Security Error"
                    errorMessage = "Invalid SSL Certificate"
                }
                
                self.showSimpleErrorAlert(title, message: errorMessage)
            }
            
            do{
                
                if serverResponse.data != nil
                {
                    let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                    
                    
                    if(json["error"] == nil)
                        
                    {
                        
                        Server.addServerUrl(urlFrontEnd: self.selectedServerFrontEndUrl, urlApi: self.selectedServerUrl, managedObjectContext: self.context)
                        Server.setNewServer(urlFrontEnd: self.selectedServerFrontEndUrl, urlApi: self.selectedServerUrl, managedObjectContext: self.context)
                        
                        self.ChooseServerButton.isHidden = false
                        
                        self.setUserInformation(userInfo: json)
                        
                        (UIApplication.shared as! MyApplication).refreshInactivityTimer(timeToExpireActivity: (UIApplication.shared as! MyApplication).inactivityInterval)
                        
                        self.getClient()
                        
                    }
                        
                    else
                        
                    {
                        let okMex = (json["error"] == nil)
                        let messageMex = !okMex ? json["error_description"] as! String : ""
                        self.showSimpleErrorAlert("Error", message: messageMex)
                    }
                    
                }
            } catch {
                print("Error deserializing JSON: \(error)")
            }
            
            self.activityIndicator.stopAnimating()
            
            self.view.isUserInteractionEnabled = true
        }
        
    }
    
    
    func showSimpleErrorAlert(_ title : String, message: String)
    {
        let alert = UIAlertController(title: title, message: message, preferredStyle: .alert)
        
        alert.addAction(UIAlertAction(title: "Ok", style: .destructive, handler:nil))
        
        self.present(alert, animated: false, completion: nil)
    }
    
    
    func getClient()
    {
        ServerConnection.sendGetClientRequest(["data":["code":UserDefaults.standard.value(forKey: "userCode") as! String, "description":""]]){ (serverResponse) -> Void in
            if(serverResponse.data != nil && serverResponse.error == nil)
            {
                self.performSegue(withIdentifier: "loggedSegue", sender: self)
            }
            else
            {
                self.showSimpleErrorAlert("Error", message: "Connection error to the server")
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
        }
    }
    
    
    func textFieldDidEndEditing(_ textField: UITextField) {
        if(textField.isEqual(serverTextField)){
            isEditingServer = false
            if (!textField.text!.isEmpty) {
                activityIndicator.startAnimating()
                self.view.isUserInteractionEnabled = false
                
                ServerConnection.getApiServerRequest(server: serverTextField.text!) { (serverResponse) -> Void in
                    if (serverResponse.error != nil) {
                        self.showSimpleErrorAlert("Error", message: "Connection error to the server\n\(String(describing: serverResponse.error))")
                    } else if (serverResponse.data != nil) {
                        do {
                            let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                            self._selectedServerFrontEndUrl = self.serverTextField.text!
                            self._selectedServerUrl = (json["API"] as! String)
                            self.companyTextField.text = ""
                            
                        } catch {
                            self.showSimpleErrorAlert("Error", message: "Cannot parse response")
                           print("Error deserializing JSON: \(error)")
                        }
                    }
                    
                    self.activityIndicator.stopAnimating()
                    self.view.isUserInteractionEnabled = true
                }
            }
        }
    }
    
    func textFieldDidBeginEditing(_ textField: UITextField) {
        if(textField.isEqual(serverTextField)){
            isEditingServer = true
        }
    }
    
    @objc func dismissKeyboard() {
        
        view.endEditing(true)
        
    }
    
}



