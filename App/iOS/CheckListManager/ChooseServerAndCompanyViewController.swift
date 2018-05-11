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
    
    var companyFieldsDictionary: [String:String]?
    
    var serverFieldsDictionary: [String:(api: String, frontEnd: String)]?
    
    var rowIndex : Int?
    
    var loginViewController: LoginViewController?
    
    var secondLine: UIView!
    
    @IBOutlet var innerView: UIView!
    
    var fieldNameString: String?
    
    var inputTextField: UITextField?
    
    var chooseServer : Bool!
    
    @IBOutlet var fieldNameLabel: UILabel!
    
    
    
    
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
        
        
        fieldLabel?.text = chooseServer ? Array(serverFieldsDictionary!.keys).reversed()[indexPath.row] : Array(companyFieldsDictionary!.keys)[indexPath.row]
        inputTextField?.isHidden = true
        
        
        cell!.preservesSuperviewLayoutMargins = false
        
        return cell!
    }
    
    
    
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        
        if(chooseServer!) {
            
            
            let url = serverFieldsDictionary![Array(serverFieldsDictionary!.keys).reversed()[indexPath.row]]!
            
            loginViewController?.serverTextField.text = url.frontEnd
            
            Server.setNewServerUrl(url, managedObjectContext: (UIApplication.shared.delegate as! AppDelegate).managedObjectContext)
            
        } else {
            loginViewController?.companyTextField.text = Array(companyFieldsDictionary!.keys)[indexPath.row]
            
            loginViewController?.selectedCompany = companyFieldsDictionary![Array(companyFieldsDictionary!.keys)[indexPath.row]]!
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
