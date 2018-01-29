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
        
        serverTextField.text = (Server.getServer((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)?.serverUrl)!
        
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
        
        ServerConnection.getCompaniesRequest(){ (serverResponse) -> Void in
            
            do{
                
                if serverResponse?.data != nil
                {
                    let json = try JSONDecoder().decode([Company].self, from: serverResponse!.data!)
                    
                    self.chooseDictionary.removeAllObjects()
                    
                    for company in json {
                        self.chooseDictionary.addEntries(from: [company.name : String(company.id)])
                    }
                    
                    print(self.chooseDictionary.description)
                    
                    self.activityIndicator.stopAnimating()
                    
                    self.performSegue(withIdentifier: "ChooseServer", sender: "company")
                    
                }
            } catch {
                print("Error deserializing JSON: \(error)")
            }
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
    
    
    
    
    // usr: "CheckListManager" pw "V4sc0 R0ss1"
    @IBAction func loginToserver(_ sender: UIButton) {
        
        if(serverTextField.text!.contains("https"))
        
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
        
        
        }
    
    
    
    func performLogin()
    
    {
        
        if(companyTextField.text! != "" && userCodeTextField.text! != "" && passwordTextField.text! != "") {
            
            activityIndicator.startAnimating()
            
            self.view.isUserInteractionEnabled = false
            
            ServerConnection.baseUrl = self.serverTextField.text!
        
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
                        
                        //print ("data received = \(json)")
                        
                        if(json["error"] == nil)
                            
                        {
                            Server.setNewServerUrl(self.serverTextField.text!, managedObjectContext: self.context)
                            
                            UserDefaults.standard.setValue(self.userCodeTextField.text, forKey: "userCode")
                            
                            UserDefaults.standard.setValue(true, forKey: "logged")
                            
                            UserDefaults.standard.setValue(json["access_token"], forKey: "access_token")
                            
                            UserDefaults.standard.setValue(json["refresh_token"], forKey: "refresh_token")
                            
                            UserDefaults.standard.setValue(json["token_type"], forKey: "token_type")
                            
                            UserDefaults.standard.setValue(Formatter.instance.dateFromTimeString(json[".expires"] as! String), forKey: "token_expires")
                            
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
        } else if(companyTextField.text! == "" ) {
            
            self.showSimpleErrorAlert("Error", message: "Select a company")
            
        } else if(userCodeTextField.text! == "") {
            
            self.showSimpleErrorAlert("Error", message: "Select a user code")
            
        } else if(passwordTextField.text! == "") {
            
            self.showSimpleErrorAlert("Error", message: "Select a password")
            
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
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        if(segue.identifier == "ChooseServer") {
            if let controller = segue.destination as? ChooseServerAndCompanyViewController {
                
                if(sender as! String == "server") {
                
                    chooseDictionary.removeAllObjects()
                    
                    chooseDictionary = ["Sviluppo": "https://itsbebewsp00642.jnj.com",
                                        "Acceptance": "https://itsbebewsp00899.jnj.com",
                                        "Production": "https://itsbebewsp00910.jnj.com",
                                        "Sviluppo2": "http://clm.matematici.com",
                                        "Sviluppo3": "http://api.clm.matematici-aws.com"]
                }
                
                controller.fieldsDictionary = chooseDictionary as? Dictionary<String,String>
                
                controller.loginViewController = self
                
                controller.chooseServer = (sender as! String) == "server" ? true : false
                
            }
        }
    }

    
    func textFieldDidEndEditing(_ textField: UITextField) {
        if(textField.isEqual(serverTextField)){
            isEditingServer = false
            ServerConnection.baseUrl = self.serverTextField.text!
        }
    }

    @objc func dismissKeyboard() {
        
        view.endEditing(true)
        
    }
    
}



