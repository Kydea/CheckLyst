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
    
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    
    var isEditingServer = false
    
    var chooseDictionary : NSMutableDictionary = [:]
    
    var selectedCompany : String = ""
    
    let context = ((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)
    
    
    override func viewDidLoad()
        
    {
        
        super.viewDidLoad()
        
        serverTextField.delegate = self
        
        serverTextField.text = (Server.getServer((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)?.serverFrontEndUrl)!
        
        userCodeTextField.delegate = self
        
        passwordTextField.delegate = self
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
        
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.keyboardWillShow(_:)), name: NSNotification.Name.UIKeyboardWillShow, object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.keyboardWillHide(_:)), name: NSNotification.Name.UIKeyboardWillHide, object: nil)
        
        let tap: UITapGestureRecognizer = UITapGestureRecognizer(target: self, action: #selector(LoginViewController.dismissKeyboard))
        
        view.addGestureRecognizer(tap)
        
        activityIndicator.hidesWhenStopped = true
        
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        
        serverTextField.addTarget(self, action: #selector(LoginViewController.chooseServer(_:)), for: UIControlEvents.touchDown)
        
    }
    
    @IBAction func chooseServer(_ sender: Any) {
        performSegue(withIdentifier: "ChooseServer", sender: "server")
    }
    
    @IBAction func chooseCompany(_ sender: Any) {
        
        activityIndicator.startAnimating()
        
        self.view.isUserInteractionEnabled = false
        
        ServerConnection.getCompaniesRequest(){ (serverResponse) -> Void in
            
            if(serverResponse.error != nil){
                
                var errorMessage = "No message"
                let title = "Error"
                
                if(serverResponse.error!.code == -1009){
                    errorMessage = "Connection error to the server"
                }
                
                self.showSimpleErrorAlert(title, message: errorMessage)
            }
            
            do{
                
                if serverResponse.data != nil
                {
                    let json = try JSONDecoder().decode([Company].self, from: serverResponse.data!)
                    
                    self.chooseDictionary.removeAllObjects()
                    
                    for company in json {
                        self.chooseDictionary.addEntries(from: [company.name : String(company.id)])
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
    }
    
    
    override func didReceiveMemoryWarning()
        
    {
        
        super.didReceiveMemoryWarning()
        
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
        
        ServerConnection.loginOrRefreshToServer(["Company" : self.selectedCompany,"Code":userCodeTextField.text!,"Password":passwordTextField.text!]){ (serverResponse) -> Void in
            
            if(serverResponse.error != nil){
                
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
                        
                        self.setUserInformation(userInfo: json)
                        
                        (UIApplication.shared as! MyApplication).refreshInactivityTimer(timeToExpireActivity: (UIApplication.shared as! MyApplication).inactivityTime)
                        
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
        
        UserDefaults.standard.setValue(Formatter.instance.dateFromTimeString(json[".expires"] as! String), forKey: "token_expires")
        
        UserDefaults.standard.setValue(json["decimalSeparator"], forKey: "decimalSeparator")
        
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        if(segue.identifier == "ChooseServer") {
            if let controller = segue.destination as? ChooseServerAndCompanyViewController {
                
                if(sender as! String == "server") {
                    
                    chooseDictionary.removeAllObjects()
                    
                    chooseDictionary = [
                        "Production": (api:"https://checklystapi.jnj.com",frontEnd: "https://checklyst.jnj.com"),
                        "Pre-Acceptance": (api:"http://10.157.83.252",frontEnd: "http://10.157.83.244"),
                        "Acceptance": (api:"https://checklystqaapi.jnj.com", frontEnd: "https://checklystqa.jnj.com"),
                        "Development": (api:"https://checklystdevapi.jnj.com", frontEnd: "https://checklystdev.jnj.com"),
                        "Development 2": (api:"http://api.clm.matematici-aws.com",frontEnd:"http://clm.matematici-aws.com")
                    ]
                    
                    controller.serverFieldsDictionary = chooseDictionary as? Dictionary<String,(api:String, frontEnd:String)>
                } else {
                    controller.companyFieldsDictionary = chooseDictionary as? Dictionary<String,String>
                }
                
                controller.loginViewController = self
                
                controller.chooseServer = (sender as! String) == "server" ? true : false
                
            }
        }
    }
    
    
    func textFieldDidEndEditing(_ textField: UITextField) {
        if(textField.isEqual(serverTextField)){
            isEditingServer = false
        }
    }
    
    @objc func dismissKeyboard() {
        
        view.endEditing(true)
        
    }
    
}



