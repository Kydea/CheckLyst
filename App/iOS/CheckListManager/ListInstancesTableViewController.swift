//
//  ListInstancesTableViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 21/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation

import UIKit
import CoreData

class ListInstancesTableViewController: UIViewController, UITableViewDataSource, UITableViewDelegate
{
    
    var instancesList: [Instance?] = []
    var buttonTappedRows : [Bool]?
    var infoButtonTapped = false
    var infoButtonTappedRow = -1
    var selectedInstance: Instance?
    
    @IBOutlet var tableView: UITableView!
    
    
    
    override func viewDidLoad() {
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        buttonTappedRows = [Bool](repeating: false, count: instancesList.count)
        self.tableView.tableFooterView =  UIView()
    }

    
    
    func numberOfSections(in tableView: UITableView) -> Int {
        return 1
    }
    
 
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return instancesList.count
    }
    
 
    func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat
     {
        
        let instance = instancesList[indexPath.row]

        if(buttonTappedRows![indexPath.row])
            
        {
            return instance!.description != nil ? 254.0 :  228.0
        }
        else if(instance!.description != nil)
        {
        return 110//130.0
            
        }
        else
        {
            return 84//104.0
        }
    }
    
    
     
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {

        let cell = tableView.dequeueReusableCell(withIdentifier: "instanceCell") as! ListInstancesCell!
       
        cell?.preservesSuperviewLayoutMargins = false
    
        let accessoryButton = UIButton(type: .roundedRect)
        
        accessoryButton.tag = indexPath.row
        
        accessoryButton.titleLabel?.font = UIFont (name: "Flaticon", size: 20)

        (!buttonTappedRows![indexPath.row]) ? accessoryButton.setTitle("\u{f107}", for: UIControlState()) :  accessoryButton.setTitle("\u{f102}", for: UIControlState())
  
        accessoryButton.frame = CGRect(x: 0, y: 0, width: 20, height: 20)
        
        accessoryButton.addTarget(self, action: #selector(ListInstancesTableViewController.accessoryButtonTapped(_:)), for: .touchUpInside)
        
        accessoryButton.contentMode = .scaleAspectFit
        
        cell?.accessoryView = accessoryButton as UIView
        
        let instance = instancesList[indexPath.row]
        
        let codeLabel = cell?.contentView.viewWithTag(10) as? UILabel
        
        let descrLabel = cell?.contentView.viewWithTag(11) as? UILabel
        
        let stateLabel = cell?.contentView.viewWithTag(12) as? UILabel
        
        let createdByLabel = cell?.contentView.viewWithTag(13) as? UILabel
        
        let createdOnLabel = cell?.contentView.viewWithTag(14) as? UILabel
        
        let modifiedbyLabel = cell?.contentView.viewWithTag(15) as? UILabel
        
        let modifiedOnLabel = cell?.contentView.viewWithTag(16) as? UILabel
        
        let idLabel = cell?.contentView.viewWithTag(17) as? UILabel
        
        let departmentPrototypeTypeLabel = cell?.contentView.viewWithTag(18) as? UILabel
        
       
        codeLabel?.text = String(instance!.prototypeName) + "." + String(instance!.version) + "." + String(instance!.year) + "." + String(instance!.number)
        
        departmentPrototypeTypeLabel?.text = instance!.departmentName + " / " + instance!.prototypeTypeName
        
        idLabel?.text = String(instance!.id)
        
        descrLabel?.text = instance!.description
        
        if (instance!.description != nil) {
            print("stop")
        }
        
        
        if(instance!.description == nil)
        
        {
            cell?.stateLabelTopConstraint.constant = 10.0
            tableView.setNeedsLayout()
        } else {
            cell?.stateLabelTopConstraint.constant = 33.0
            tableView.setNeedsLayout()
        }
        
        stateLabel?.text = instance!.state
        
        createdByLabel?.text = instance!.createdBy

        
        if(instance!.createdOn != nil)
            
        {
            createdOnLabel?.text = Formatter.instance.stringFromServerToReadbleString(instance!.createdOn!)
        }

        else
            
        {
            createdOnLabel?.text = "no date"

        }

        modifiedbyLabel?.text = instance!.modifiedBy
 
        if(instance!.modifiedOn != nil)
            
        {
            modifiedOnLabel?.text = Formatter.instance.stringFromServerToReadbleString(instance!.modifiedOn!)
        }
 
        else
            
        {
            modifiedOnLabel?.text = "no date"
        }
        
        
        return cell!
 
    }

    @objc func accessoryButtonTapped(_ sender : AnyObject){
    
        
        var paths: [IndexPath] = []
        
        let oldIndexPath = IndexPath(row: infoButtonTappedRow, section: 0)
        
        paths.append(IndexPath(row: sender.tag, section: 0))
        
        paths.append(oldIndexPath)
        
        if(infoButtonTappedRow != -1 && infoButtonTappedRow != sender.tag)
            
        {
            if(buttonTappedRows![infoButtonTappedRow])
                
            {
                
                buttonTappedRows![infoButtonTappedRow] = !buttonTappedRows![infoButtonTappedRow]
            }
            
        }
 
        infoButtonTappedRow = sender.tag
        
        buttonTappedRows![sender.tag] = !buttonTappedRows![sender.tag]
        
        tableView.beginUpdates()
        
        tableView.reloadRows(at: paths, with: UITableViewRowAnimation.automatic)
        
        tableView.endUpdates()
        
    }
    

    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        
        selectedInstance = instancesList[indexPath.row]
        
        performSegue(withIdentifier: "instanceDetails", sender: self)
        
    }
    
    
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        
        if(segue.identifier == "instanceDetails")
            
        {
            
            if let controller = segue.destination as? InstanceViewController {
                
                controller.instance = selectedInstance
                
            }
   
        }
    }
}

