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

class ListInstancesTableViewController: UIViewController, UITableViewDataSource, UITableViewDelegate, UITableViewDataSourcePrefetching {
    
    var instancesList: [Instance?] = []
    var numberOfPages = 0
    var itemsPerPage = 0
    var totalItems = 0
    var lastDownloadedPage = 1 //from 1 beacuse user get here only if there is at least one page
    var buttonTappedRows : [Bool]?
    var infoButtonTapped = false
    var infoButtonTappedRow = -1
    var selectedInstance: Instance?
    var selectedItemsDictionary = [Int:Option] ()
    var searchDictionary : Dictionary<String, Any?>?
    let appDelegate = UIApplication.shared as! MyApplication
    var isFetching = false
    
    @IBOutlet var tableView: UITableView!
    
    override func viewDidLoad() {
        
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        buttonTappedRows = [Bool](repeating: false, count: instancesList.count)
        self.tableView.tableFooterView =  UIView()
    
    }

    override func viewDidAppear(_ animated: Bool) {
        
        super.viewDidAppear(animated);
        
    }
    
    func numberOfSections(in tableView: UITableView) -> Int {
        
        return 1
    
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        
        return self.totalItems
    
    }
    
    func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
        
        if isLoadingCell(for: indexPath) {
            
            return 0
        
        }
        
        let descrLabelWidth = UIScreen.main.bounds.size.width - 140
        
        let instance = instancesList[indexPath.row]
        
        var descriptionSize : CGFloat = 0
        
        if let description = instance?.description?.classFormat {
            descriptionSize = description.height(withConstrainedWidth: descrLabelWidth)
        }
        
        
        if(buttonTappedRows![indexPath.row]) {
            
            return instance!.description != nil ? 239.0 + descriptionSize :  239.0
        
        } else if(instance!.description != nil) {
        
            return 95 + descriptionSize
            
        } else {
            
            return 95
        
        }
    
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {

        let cell = tableView.dequeueReusableCell(withIdentifier: "instanceCell") as? ListInstancesCell
        
        cell?.setLayout()
        
        if !isLoadingCell(for: indexPath) {
          
            cell?.contentView.isHidden = false
            
            cell?.preservesSuperviewLayoutMargins = false
        
            let accessoryButton = UIButton(type: .roundedRect)
            
            accessoryButton.tag = indexPath.row
            
            accessoryButton.titleLabel?.font = UIFont (name: "Flaticon", size: 20)

            (!buttonTappedRows![indexPath.row]) ? accessoryButton.setTitle("\u{f107}", for: UIControlState()) :  accessoryButton.setTitle("\u{f102}", for: UIControlState())
      
            accessoryButton.frame = CGRect(x: 0, y: 0, width: 20, height: 20)
            
            accessoryButton.tintColor = UIColor(red: 0.0/255, green: 150.0/255, blue: 136.0/255, alpha: 1.0)
            
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
            
            if (instance!.description?.classFormat != nil) {
                
                descrLabel?.attributedText = instance!.description?.classFormat
           
            } else {
            
                descrLabel?.text = ""
            
            }
            
            stateLabel?.text = instance!.state
            
            createdByLabel?.text = instance!.createdBy

            
            if(instance!.createdOn != nil) {
                
                createdOnLabel?.text = Formatter.instance.stringFromServerToReadbleString(instance!.createdOn!)
            
            } else {
                
                createdOnLabel?.text = "no date"

            }

            modifiedbyLabel?.text = instance!.modifiedBy
     
            if(instance!.modifiedOn != nil) {
             
                modifiedOnLabel?.text = Formatter.instance.stringFromServerToReadbleString(instance!.modifiedOn!)
           
            } else {
                
                modifiedOnLabel?.text = "no date"
            
            }
        
        }
        return cell!
 
    }

    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
          
          selectedInstance = instancesList[indexPath.row]
          
          performSegue(withIdentifier: "instanceDetails", sender: self)
          
    }
    
    func tableView(_ tableView: UITableView, prefetchRowsAt indexPaths: [IndexPath]) {
          
        if indexPaths.contains(where: isLoadingCell) {
        
            getResearchResultForPage(page: self.lastDownloadedPage + 1)
          
        }
    
    }
    
    fileprivate func isLoadingCell(for indexPath: IndexPath) -> Bool {
    
        return indexPath.row >= instancesList.count
    
    }

    fileprivate func visibleIndexPathsToReload(intersecting indexPaths: [IndexPath]) -> [IndexPath] {
       
        let indexPathsForVisibleRows = tableView.indexPathsForVisibleRows ?? []
        let indexPathsIntersection = Set(indexPathsForVisibleRows).intersection(indexPaths)
        return Array(indexPathsIntersection)
    
    }
    
    fileprivate func calculateIndexPathsToReload(from newIstances: [Instance]) -> [IndexPath] {
       
        let startIndex = instancesList.count - newIstances.count
        let endIndex = startIndex + newIstances.count
        return (startIndex..<endIndex).map { IndexPath(row: $0, section: 0) }
    
    }
    
    @objc func accessoryButtonTapped(_ sender : AnyObject){
    
        var paths: [IndexPath] = []
        
        let oldIndexPath = IndexPath(row: infoButtonTappedRow, section: 0)
        
        paths.append(IndexPath(row: sender.tag, section: 0))
        
        if(oldIndexPath.row != -1) {
            
            paths.append(oldIndexPath)
       
        }
        
        if(infoButtonTappedRow != -1 && infoButtonTappedRow != sender.tag) {
           
            if(buttonTappedRows![infoButtonTappedRow]) {
                
                buttonTappedRows![infoButtonTappedRow] = !buttonTappedRows![infoButtonTappedRow]
           
            }
            
        }
 
        infoButtonTappedRow = sender.tag
        
        buttonTappedRows![sender.tag] = !buttonTappedRows![sender.tag]
        
        tableView.beginUpdates()
        
        tableView.reloadRows(at: paths, with: UITableViewRowAnimation.automatic)
        
        tableView.endUpdates()
        
    }
    
    func getResearchResultForPage(page:Int) {
        
        if (isFetching) {
            
            return
       
        }
        
        isFetching = true
        
        ServerConnection.getListInstancesWithParams(["CurrentPage" : String(page), "data" : self.searchDictionary!]) { (serverResponse) -> Void in
            
            do {
                
                if(serverResponse.data != nil) {
                    
                    let listIstancesJSON = try JSONDecoder().decode(ListIstancesJSON.self, from: serverResponse.data!)

                    self.instancesList += listIstancesJSON.data
                    
                    self.lastDownloadedPage += 1
                    
                    self.buttonTappedRows?.append(contentsOf: [Bool](repeating: false, count: self.instancesList.count))
                    
                    let newIndexPathsToReload = self.calculateIndexPathsToReload(from: listIstancesJSON.data as! [Instance])
                    
                    let indexPathsToReload = self.visibleIndexPathsToReload(intersecting: newIndexPathsToReload)
                    
                    self.tableView.reloadRows(at: indexPathsToReload, with: .none)
                                        
                } else {
                    
                    self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server") { () -> Void in self.tabBarController?.selectedIndex = 0 }
                    
                }
                
              self.isFetching = false
                
            } catch {
                
                print(error)
                
                do {
                   
                    var error = String()
                    
                    let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                    
                    if let errorMessages = json["messages"] as? [String] {
                    
                        for value in errorMessages {
                        
                            error.append(value + "\n")
                        
                        }
                        
                        self.appDelegate.displayMessage(Title: "Error", Message: error) { () -> Void in self.tabBarController?.selectedIndex = 0 }
                    
                    } else {
                    
                        self.appDelegate.displayMessage(Title: "Error", Message: "Unexpected error has occurred") { () -> Void in self.tabBarController?.selectedIndex = 0 }
                    
                    }
                    
                    self.isFetching = false
                    
                } catch {
                    
                    print(error)
                    
                    self.isFetching = false

                    
                }
            }
        }
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        
        if(segue.identifier == "instanceDetails") {
            
            if let controller = segue.destination as? InstanceViewController {
                
                controller.instance = selectedInstance
                controller.selectedItemsDictionary = selectedItemsDictionary;
            
            }
   
        }
    }
}

