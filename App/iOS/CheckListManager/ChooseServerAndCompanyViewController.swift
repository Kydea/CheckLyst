//
//  ChooseServerViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 24/02/17.
//  Copyright © 2017 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

class ChooseServerAndCompanyViewController: UIViewController, UITableViewDelegate, UITableViewDataSource {
    
    @IBOutlet var tableView: UITableView!
    
    @IBOutlet var okButton: UIButton!
    
    var companyFieldsDictionary: [String:[String: String]]?
    
    var serverFieldsDictionary: [Server]?
    
    var rowIndex : Int?
    
    var loginViewController: LoginViewController?
    
    var secondLine: UIView!
    
    @IBOutlet var innerView: UIView!
    
    var fieldNameString: String?
    
    var inputTextField: UITextField?
    
    var chooseServer : Bool!
    
    @IBOutlet var fieldNameLabel: UILabel!
    
    let context = ((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)
    
    override func viewDidLoad() {
        
        fieldNameLabel?.text = fieldNameString
        
        okButton.isHidden = true
        
    }
    
    func numberOfSections(in tableView: UITableView) -> Int {
        
        return 1
        
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        
        return (chooseServer ? serverFieldsDictionary?.count : companyFieldsDictionary?.count) ?? 1
    
    }
    
    
    
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        
        let cell = tableView.dequeueReusableCell(withIdentifier: "searchCell")
        
        let fieldLabel = cell!.contentView.viewWithTag(1) as? UILabel
        
        inputTextField = cell!.contentView.viewWithTag(2) as? UITextField
        
        
        fieldLabel?.text = chooseServer ? serverFieldsDictionary![indexPath.row].serverFrontEndUrl : Array(companyFieldsDictionary!.keys)[indexPath.row]
        inputTextField?.isHidden = true
        
        
        cell!.preservesSuperviewLayoutMargins = false
        
        return cell!
    }
    
    
    
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        
        if(chooseServer!) {
            
            let server = serverFieldsDictionary![indexPath.row]
            
            loginViewController?.serverTextField.text = server.serverFrontEndUrl
            
            loginViewController?.textFieldDidEndEditing(loginViewController!.serverTextField)
            
        } else {
            loginViewController?.companyTextField.text = Array(companyFieldsDictionary!.keys)[indexPath.row]
            
            let id = Int(companyFieldsDictionary![Array(companyFieldsDictionary!.keys)[indexPath.row]]!["id"]!)!
            let name = Array(companyFieldsDictionary!.keys)[indexPath.row]
            let inactivitySecondsTimeout = Int(companyFieldsDictionary![Array(companyFieldsDictionary!.keys)[indexPath.row]]!["inactivityTimeoutSeconds"]!)!
            
            loginViewController?.selectedCompany = Company(id: id, name: name, inactivityTimeoutSeconds: inactivitySecondsTimeout)
        }
        
        self.navigationController?.popViewController(animated: true)
        
    }
    
    
    override func viewWillDisappear(_ animated : Bool) {
        
        super.viewWillDisappear(animated)
        
    }
    
    
    
    @IBAction func saveEditing(_ sender: AnyObject) {
        
        self.navigationController?.popViewController(animated: true)
    }
    
    
}
