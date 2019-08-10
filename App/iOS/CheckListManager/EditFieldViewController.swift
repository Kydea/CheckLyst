//
//  EditFieldViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 08/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit


class EditFieldViewController: UIViewController, UITableViewDataSource, UITableViewDelegate
    
    
{

    @IBOutlet var tableView: UITableView!
    
    
    @IBOutlet var okButton: UIButton!
 
    var fieldsArray: [Option]?
    
    var rowIndex : Int?
    
    var searchViewController: InstancesSearchViewController?
    
    var isListField: Bool?
    
    var secondLine: UIView!
    
    @IBOutlet var innerView: UIView!
 
    var fieldNameString: String?
    
    var inputTextField: UITextField?
    
    var indexFieldDictionary: Dictionary<Int, String>?
    
    @IBOutlet var fieldNameLabel: UILabel!
    
  

    
    override func viewDidLoad() {
        
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        
        fieldNameLabel?.text = fieldNameString
        
        okButton.isHidden = (isListField!) ? true : false
        
        if(!isListField!)
        
        {
            tableView.allowsSelection = false
        }
    }
    


    
    
    func numberOfSections(in tableView: UITableView) -> Int {
        
        return 1
        
    }
    
    
 
    
    
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        
        
        return fieldsArray?.count ?? 1
    }
    
    
    
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        
        
        let cell = tableView.dequeueReusableCell(withIdentifier: "searchCell")
        
        let fieldLabel = cell!.contentView.viewWithTag(1) as? UILabel
        
        inputTextField = cell!.contentView.viewWithTag(2) as? UITextField
        
        if(isListField!)
        
        {
          
            fieldLabel?.text = fieldsArray![indexPath.row].text
        
            inputTextField?.isHidden = true
            
        }
        
        else
        
        {
            
        fieldLabel?.text = searchViewController?.indexTextDictionary[rowIndex!]
        
        inputTextField?.text = (searchViewController?.selectedItemsDictionary[rowIndex!]?.text != "Insert") ?  searchViewController?.selectedItemsDictionary[rowIndex!]?.text: ""
            
        }
        
        cell!.preservesSuperviewLayoutMargins = false
        
        if(searchViewController!.selectedItemsDictionary[rowIndex!]?.text == fieldLabel?.text && isListField!)
        
        {
         
        let accessoryButton = UIButton(type: .roundedRect)
        
        accessoryButton.tag = indexPath.row
        
        accessoryButton.titleLabel?.font = UIFont (name: "Flaticon", size: 25)
        
        accessoryButton.tintColor = UIColor(red: 0.0/255, green: 150.0/255, blue: 136.0/255, alpha: 1.0)
        
        accessoryButton.setTitle("\u{f101}", for: UIControlState())
        
        accessoryButton.frame = CGRect(x: 0, y: 0, width: 30, height: 30)
        
        accessoryButton.contentMode = .scaleAspectFit
  
        cell!.accessoryView = accessoryButton as UIView
        
        }
        
        else
        
        {
            cell!.accessoryView = UIView()
        }

        return cell!
    }
    
    
    
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        
        if(isListField!)
        
        {
            
            if(searchViewController!.selectedItemsDictionary[rowIndex!]?.text != fieldsArray![indexPath.row].text && rowIndex! < 7) {
                for i in (rowIndex! + 1)...7 {
                    if(indexFieldDictionary![i] != "description" && indexFieldDictionary![i] != "state") {
                        searchViewController!.selectedItemsDictionary[i]?.text = ""
                        searchViewController!.selectedItemsDictionary[i]?.value = ""
                    }
                }
            }
            
            searchViewController!.selectedItemsDictionary[rowIndex!] = fieldsArray![indexPath.row]
            
        
            self.navigationController?.popViewController(animated: true)
        }
        
        
    }
    
    
    override func viewWillDisappear(_ animated : Bool) {
        
        super.viewWillDisappear(animated)
        
        if (self.isMovingFromParentViewController && !isListField!){
            
            searchViewController!.selectedItemsDictionary[rowIndex!]?.text = (inputTextField?.text == "" ? "Insert" : (inputTextField?.text)!)
            
            searchViewController!.selectedItemsDictionary[rowIndex!]?.value = (inputTextField?.text == "" ? "" : (inputTextField?.text)!)
            
        }
    
    }
    

    
    @IBAction func saveEditing(_ sender: AnyObject) {
        
        self.navigationController?.popViewController(animated: true)
    }




}
