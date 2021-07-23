//
//  InstancesSearchViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 21/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import UIKit


class InstancesSearchViewController: UIViewController,UIScrollViewDelegate, UITextFieldDelegate, UITabBarDelegate, UITableViewDataSource, UITableViewDelegate {
    
    @IBOutlet var tableView: UITableView!
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
    @IBOutlet var scrollView: UIScrollView!
    var listInstances : [Instance?] = []
    @IBOutlet var newInstanceButton: UIBarButtonItem!
    @IBOutlet var searchInstanceButton: UIButton!
    
    /*  static text for the labels */
    var indexTextDictionary =
        [0: "Instance ID", 1:"Departments", 2:"Prototype Types", 3:"Prototype", 4:"Version", 5:"Description", 6:"Tags", 7:"Tags Value", 8:"States"]
    
    /*  values sent to the server like query    */
    var searchDictionary : Dictionary<String, Any?> = ["id": "", "department" : "", "description" : "", "prototype" : "", "prototypeType": "", "state" : "", "tagCode" : "", "tagValue" : "", "version" : "", "from" : nil, "to" : nil, "advancedSearch" : false, "source" : nil]
    
    var fieldIndexDictionary =
        ["id": 0, "department":1, "prototypeType":2, "prototype":3, "version":4, "description":5, "tagCode":6, "tagValue":7 ,"state":8]
    
    var indexFieldDictionary = [0:"id", 1:"department", 2:"prototypeType", 3:"prototype", 4:"version", 5:"description", 6:"tagCode", 7:"tagValue", 8:"state"]
    
    let allowedSearchFieldValues = ["department","prototypeType","prototype","version"]
    
    var selectedItemsDictionary = [Int:Option] ()
    var indexArrayDictionary = [Int:[Option?]] ()
    var statesArray: [String] = []
    
    var numberOfPages = 0
    var itemsPerPage = 0
    var totalItems = 0
    
    let appDelegate = UIApplication.shared as! MyApplication
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        navigationItem.setRightBarButtonItems([infoBarButtonItem,navigationItem.rightBarButtonItem!], animated: false)

        self.tableView.tableFooterView =  UIView()
        self.tableView.isScrollEnabled = false
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        activityIndicator.hidesWhenStopped = true
        newInstanceButton.title = "\u{f103} New Instance "
        newInstanceButton.isEnabled = checkRoles(role: "Instances#Create#")
        
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
        
        for i in 0...8 {
            selectedItemsDictionary[i] = Option(text: "",value: "")
        }
        loadFilters()
    }
    
    
    override func viewDidAppear(_ animated: Bool) {

        if appDelegate.requestToOpenInstance {
            appDelegate.openInstance()
            return
        }
        
        fromSelectionToSearchDictionary()
        saveFilters()
        fillSearchFields()
    }
    
    func fromSelectionToSearchDictionary() {
        for field in searchDictionary.keys {
            if let index = fieldIndexDictionary[field] {
                searchDictionary[field] = (field == "version") ? selectedItemsDictionary[index]!.value.components(separatedBy: " -")[0] : searchDictionary[field]
                searchDictionary[field] = (searchDictionary[field] == nil) ? "" : selectedItemsDictionary[fieldIndexDictionary[field]!]!.value
            }
        }
    }
    
    
    func numberOfSections(in tableView: UITableView) -> Int {
        return 1
    }
    
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return fieldIndexDictionary.count
    }
    
    private func shouldUseSlimLayout() -> Bool {
        let currentDevice = UIDevice.current
        return currentDevice.userInterfaceIdiom == .phone &&
            (currentDevice.orientation == .portrait || currentDevice.orientation == .portraitUpsideDown)
        
    }
    
    override func viewWillTransition(to size: CGSize, with coordinator: UIViewControllerTransitionCoordinator) {
            super.viewWillTransition(to: size, with: coordinator)
            tableView.reloadData()
        }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        
        let cell = tableView.dequeueReusableCell(withIdentifier: "searchCell") as? InstanceSearchCell
        
        cell?.setLayout()
        
        let accessoryButton = UIButton(type: .roundedRect)
        accessoryButton.tag = indexPath.row
        accessoryButton.titleLabel?.font = UIFont (name: "Flaticon", size: 25)
        accessoryButton.tintColor = UIColor(red: 0.0/255, green: 150.0/255, blue: 136.0/255, alpha: 1.0)
        accessoryButton.setTitle("\u{f104}", for: UIControlState())
        accessoryButton.frame = CGRect(x: 0, y: 0, width: 30, height: 30)
        accessoryButton.contentMode = .scaleAspectFit
        accessoryButton.addTarget(self, action: #selector(InstancesSearchViewController.accessoryButtonTapped(_:)), for: .touchUpInside)
        cell!.accessoryView = accessoryButton as UIView
        let fieldLabel = cell!.contentView.viewWithTag(1) as? UILabel
        fieldLabel?.text = indexTextDictionary[indexPath.row]
        let fieldValue = cell!.contentView.viewWithTag(2) as? UILabel
        fieldValue?.textAlignment = shouldUseSlimLayout() ? .left : .right
       
        fieldValue?.text = selectedItemsDictionary[indexPath.row]?.text
        cell?.isUserInteractionEnabled = true
        fieldValue!.isEnabled = true;
        
        let isList = indexTextDictionary[indexPath.row] != "Description" && indexTextDictionary[indexPath.row] != "Tags Value" && indexTextDictionary[indexPath.row] != "Instance ID"

        fieldValue?.textColor = (isList) ? UIColor.black : fieldValue?.textColor
        
        var blocked: Bool = indexArrayDictionary[indexPath.row] != nil && indexArrayDictionary[indexPath.row]!.count <= 1 ? true : false

        
        if (selectedItemsDictionary[fieldIndexDictionary["version"]!]?.text == "" && indexPath.row == fieldIndexDictionary["tagCode"]!) {
            blocked = true
            selectedItemsDictionary[fieldIndexDictionary["tagCode"]!]?.text = ""
            fieldValue?.text = ""
        }
        
        if (indexPath.row == fieldIndexDictionary["id"]) {
            if(selectedItemsDictionary[fieldIndexDictionary["id"]!]?.text == "") {
                selectedItemsDictionary[fieldIndexDictionary["id"]!]?.text = "Insert"
                fieldValue?.text = selectedItemsDictionary[indexPath.row]?.text
            }
        }
        
        if (indexPath.row == fieldIndexDictionary["description"]) {
            if(selectedItemsDictionary[fieldIndexDictionary["description"]!]?.text == "") {
                selectedItemsDictionary[fieldIndexDictionary["description"]!]?.text = "Insert"
                fieldValue?.text = selectedItemsDictionary[indexPath.row]?.text
            }
        }
  
        if (indexPath.row == fieldIndexDictionary["tagValue"]) {
            if((selectedItemsDictionary[fieldIndexDictionary["tagCode"]!]?.text == "All" || selectedItemsDictionary[fieldIndexDictionary["tagCode"]!]?.text == "")) {
                
                selectedItemsDictionary[fieldIndexDictionary["tagValue"]!]?.text = ""
                fieldValue?.text = ""
                blocked = true
            
            } else if(selectedItemsDictionary[fieldIndexDictionary["tagValue"]!]?.text == "") {
                
                blocked = false
                selectedItemsDictionary[fieldIndexDictionary["tagValue"]!]?.text = "Insert"
                fieldValue?.text = "Insert"
            
            }
        }
        
        if (blocked) {
            
            cell?.isUserInteractionEnabled = false
            fieldValue!.isEnabled = false;
            accessoryButton.setTitle("\u{f100}", for: UIControlState())
        
        }
        
        if (!isList) {
            
            accessoryButton.setTitle("", for: UIControlState())
        
        }
        
        return cell!
    }
    

    func textFieldShouldReturn(_ textField: UITextField) -> Bool {
        
        textField.resignFirstResponder()
        return true
    
    }
    

    
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
      
        performSegue(withIdentifier: "editFieldSegue", sender: indexPath)
    
    }
    
    @objc func accessoryButtonTapped(_ sender : AnyObject) {
        
        let indexPath = IndexPath(row: sender.tag, section: 0)
        performSegue(withIdentifier: "editFieldSegue", sender: indexPath)

    }
    
    @IBAction func newInstanceButtonTapped(_ sender: Any) {
        guard let prototype = searchDictionary["prototype"] as? String,
              !prototype.isEmpty else {
            appDelegate.displayMessage(Title: "Error", Message: "Select a prototype")
            return
        }
        
        checkCreate(prototype: prototype) { [weak self] in
            self?.performSegue(withIdentifier: "newInstanceSegue", sender: nil)
        }
        
    }
    
    func checkCreate(prototype: String, completition: (() -> Void)? = nil) {
    
        self.startActivityIndicator()
        
        ServerConnection.checkCreationGrants(["prototype" : prototype]) { [weak self]
            (serverResponse) in
            self?.stopActivityIndicator()
           
            if serverResponse.error != nil {
                self?.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
                return
            }
            
            if let response = serverResponse.response as? HTTPURLResponse, response.statusCode != 200 {
                
                let json = try? JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                var error = String()
                
                if let errorMessages = json?["messages"] as? [String] {
                    for value in errorMessages {
                        error.append(value + (errorMessages.last == value ? "" : "\n"))
                    }
                }
                
                if error.isEmpty {
                    error = json?["message"] as? String ?? "Unexpected error has occurred"
                }
                  

                self?.appDelegate.displayMessage(Title: "Error", Message: error)
                
                return
            }
            
            completition?()
        }
    }
   
    
    func fillSearchFields() {
        
        self.startActivityIndicator()
        
        ServerConnection.bindListWithParams(["data" : searchDictionary]) { (serverResponse) -> Void in
            
            do {
                
                if (serverResponse.data != nil) {
                    
                    let searchFields = try JSONDecoder().decode(IstanceBindResponseJSON.self, from: serverResponse.data!)
                    
                    if (searchFields.valid) {

                        if (searchFields.fields.department != nil) {
                
                            guard let departments = searchFields.fields.department?.options else {
                                print("No departments")
                                self.stopActivityIndicator()
                                return
                            }
                        
                            let index = self.fieldIndexDictionary["department"]!
                            let field = self.indexFieldDictionary[index]!
                            
                            self.indexArrayDictionary[index] = departments
                            self.indexArrayDictionary[self.fieldIndexDictionary["department"]!]?.insert(Option(text: "All", value: ""), at: 0)
                            
                            if let value = searchFields.fields.department?.value {
                            
                                print("value from search \(value)")
                                self.selectedItemsDictionary[index] = self.findOptionInIndexArrayDictionaryForValue(value, inField: field, andIndex: index)!;
                            
                            }
                        }
                        
                        if (searchFields.fields.prototypeType != nil) {
                           
                            guard let prototypeTypes = searchFields.fields.prototypeType?.options else {
                                print("No prototypeTypes")
                                self.stopActivityIndicator()
                                return
                            }
                            
                            let index = self.fieldIndexDictionary["prototypeType"]!
                            let field = self.indexFieldDictionary[index]!
                            
                            self.indexArrayDictionary[index] = prototypeTypes
                            self.indexArrayDictionary[self.fieldIndexDictionary["prototypeType"]!]?.insert(Option(text: "All", value: ""), at: 0)
                            
                            if let value = searchFields.fields.prototypeType?.value {
                            
                                print("value from search \(value)")
                                self.selectedItemsDictionary[index] = self.findOptionInIndexArrayDictionaryForValue(value, inField: field, andIndex: index)!;
                            
                            }
                        }
                        
                        if (searchFields.fields.prototype != nil) {
                            
                            guard let prototype = searchFields.fields.prototype?.options else {
                                print("No prototype")
                                self.stopActivityIndicator()
                                return
                            }
                            
                            let index = self.fieldIndexDictionary["prototype"]!
                            let field = self.indexFieldDictionary[index]!
                            
                            self.indexArrayDictionary[self.fieldIndexDictionary["prototype"]!] = prototype
                            self.indexArrayDictionary[self.fieldIndexDictionary["prototype"]!]?.insert(Option(text: "All", value: ""), at: 0)
                            
                            if let value = searchFields.fields.prototype?.value {
                            
                                print("value from search \(value)")
                                self.selectedItemsDictionary[index] = self.findOptionInIndexArrayDictionaryForValue(value, inField: field, andIndex: index)!;
                           
                            }
                        }
                        
                        if (searchFields.fields.version != nil) {
                        
                            guard let versions = searchFields.fields.version?.options else {
                                print("No versions")
                                self.stopActivityIndicator()
                                return
                            }
                            
                            let index = self.fieldIndexDictionary["version"]!
                            let field = self.indexFieldDictionary[index]!
                            
                            self.indexArrayDictionary[self.fieldIndexDictionary["version"]!] = versions
                            self.indexArrayDictionary[self.fieldIndexDictionary["version"]!]?.insert(Option(text: "All", value: ""), at: 0)
                            
                            if let value = searchFields.fields.version?.value {
                               
                                print("value from search \(value)")
                                self.selectedItemsDictionary[index] = self.findOptionInIndexArrayDictionaryForValue(value, inField: field, andIndex: index)!;
                            
                            }
                        }
                        
                        if (searchFields.fields.tagCode != nil) {
                        
                            guard let tags = searchFields.fields.tagCode?.options else {
                                print("No tags")
                                self.stopActivityIndicator()
                                return
                            }
                            
                            let index = self.fieldIndexDictionary["tagCode"]!
                            let field = self.indexFieldDictionary[index]!
                        
                            self.indexArrayDictionary[self.fieldIndexDictionary["tagCode"]!] = tags
                            self.indexArrayDictionary[self.fieldIndexDictionary["tagCode"]!]?.insert(Option(text: "All", value: ""), at: 0)
                            
                            if let value = searchFields.fields.tagCode?.value {
                              
                                //print("value from search \(value)")
                                self.selectedItemsDictionary[index] = self.findOptionInIndexArrayDictionaryForValue(value, inField: field, andIndex: index)!;
                            
                            }
                        }
                        
                        if (searchFields.fields.state != nil) {
                            
                            guard let states = searchFields.fields.state?.options else {
                                print("No state")
                                self.stopActivityIndicator()
                                return
                            }
                            
                            let index = self.fieldIndexDictionary["state"]!
                            let field = self.indexFieldDictionary[index]!
                            
                            self.indexArrayDictionary[self.fieldIndexDictionary["state"]!] = states
                            self.indexArrayDictionary[self.fieldIndexDictionary["state"]!]?.insert(Option(text: "All", value: ""), at: 0)
                            
                            if let value = searchFields.fields.state?.value {
                                print("value from search \(value)")
                                self.selectedItemsDictionary[index] = self.findOptionInIndexArrayDictionaryForValue(value, inField: field, andIndex: index)!;
                            }
                        }
                    
                        self.setLabelTitles()
                    
                    } else {
                       
                        var error = String()
                        let validationMessages = searchFields.validationMessages
                        if (validationMessages.count > 0) {
                            for value in validationMessages {
                                error.append(value! + "\n")
                            }
                            self.appDelegate.displayMessage(Title: "Error", Message: error)
                            self.tableView.isUserInteractionEnabled = false
                            self.searchInstanceButton.isUserInteractionEnabled = false
                        }
                    
                    }
                } else {
                    self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
                }
            } catch {
                
                print(error)
                
                let json = try? JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                var error = String()
                
                if let errorMessages = json?["messages"] as? [String] {
                    for value in errorMessages {
                        error.append(value + "\n")
                    }
                }
                
                if error.isEmpty {
                    error = json?["message"] as? String ?? "Unexpected error has occurred"
                }
                
                
                self.appDelegate.displayMessage(Title: "Error", Message: error)
            }
            self.stopActivityIndicator()
        }
    }
    
    func findOptionInIndexArrayDictionaryForValue(_ value:String, inField field:String, andIndex index:Int) -> Option? {
        for element in indexArrayDictionary[index]! {
            if(element!.value == value) {
                return element
            }
        }
        return nil
    }

    
    func setLabelTitles()
    {
        var selectedDepartemnt = ""
        var selectedPrototypeType = ""
        
        for index in indexArrayDictionary.keys{
            
            let field = indexFieldDictionary[index]!
            
            if let option = findOptionInIndexArrayDictionaryForValue(self.searchDictionary[field]!! as! String, inField: field, andIndex: index)
            {
                if(field == "department") { selectedDepartemnt = option.value }
                if(field == "prototypeType") { selectedPrototypeType = option.value }
                self.selectedItemsDictionary[index]?.text = option.text
                self.selectedItemsDictionary[index]?.value = option.value
            }
                
            else
            {
                if(indexArrayDictionary[index]!.count == 0) {
                    self.selectedItemsDictionary[index] = Option(text: "", value: "")
                }
            }
        }
    
        self.tableView.reloadData()
    }
    
    func checkRoles(role:String) -> Bool {
        let roles = (UserDefaults.standard.value(forKey: "roles") as! String)
        return roles.contains(role)
    }
    
    @IBAction func searchInstances(_ sender: AnyObject) {
        
        self.startActivityIndicator()
        
        self.saveFilters()
        self.listInstances = []
        self.getResearchResultForFirstPage()
        
    }
    
    func getResearchResultForFirstPage() {
        
        ServerConnection.getListInstancesWithParams(["CurrentPage" : "1", "data" : searchDictionary]) { (serverResponse) -> Void in
            
            do {
                
                if(serverResponse.data != nil) {
                    
                    let listIstancesJSON = try JSONDecoder().decode(ListIstancesJSON.self, from: serverResponse.data!)

                    self.listInstances += listIstancesJSON.data
                    
                    self.numberOfPages = listIstancesJSON.pagesCount
                    
                    self.itemsPerPage = listIstancesJSON.itemsPerPage
                    
                    self.totalItems = listIstancesJSON.count
                    
                    self.stopActivityIndicator()
                    
                    if (self.listInstances.count == 0) {
                    
                        self.stopActivityIndicator()
                        self.appDelegate.displayMessage(Title: "Error", Message: "No records found")
                    
                    } else {
                        
                        self.performSegue(withIdentifier: "instanceDetailsSegue", sender: self)
                    
                    }
                    
                } else {
                    
                    self.appDelegate.displayMessage(Title: "Error", Message: "Connection error to the server")
                    self.stopActivityIndicator()
                
                }
            } catch {
                
                print(error)
                
                do {
                    
                    var error = String()
                    let json = try JSONSerialization.jsonObject(with: serverResponse.data!, options: .allowFragments) as! [String:Any]
                    
                    if let errorMessages = json["messages"] as? [String] {
                    
                        for value in errorMessages {
                            error.append(value + "\n")
                        }
                        
                        self.appDelegate.displayMessage(Title: "Error", Message: error)
                    
                    } else {
                        
                        self.appDelegate.displayMessage(Title: "Error", Message: "Unexpected error has occurred")
                    
                    }
                    
                } catch {
                    
                    print(error)
                    
                }
            }
        }
    }
    

    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        
        if let controller = segue.destination as? EditFieldViewController {
            
            let indexPath = sender as! IndexPath
            controller.fieldsArray = indexArrayDictionary[indexPath.row] as? [Option]
            controller.searchViewController = self
            controller.rowIndex = indexPath.row
            controller.isListField = (indexTextDictionary[indexPath.row] == "Description" || indexTextDictionary[indexPath.row] == "Instance ID" || indexTextDictionary[indexPath.row] == "Tags Value") ? false : true
            controller.fieldNameString = indexTextDictionary[indexPath.row]
            controller.indexFieldDictionary = indexFieldDictionary
            if let sourceValue = indexFieldDictionary[indexPath.row], allowedSearchFieldValues.contains(sourceValue)
            {
                searchDictionary["source"] = sourceValue
            }
        
        } else if let controller = segue.destination as? ListInstancesTableViewController {
            
            controller.instancesList = self.listInstances as! [Instance]
            controller.selectedItemsDictionary = selectedItemsDictionary;
            controller.numberOfPages = self.numberOfPages
            controller.searchDictionary = self.searchDictionary
            controller.itemsPerPage = self.itemsPerPage
            controller.totalItems = self.totalItems
        
        } else if let controller = segue.destination as? InstanceViewController {
            if ((sender as AnyObject).isKind(of: MyApplication.self)) {
                
                let app = sender as! MyApplication
                if let parameters = app.requestedUrlInfo {
                    
                    if selectedItemsDictionary.count == 0  {
                        for i in 0...8 {
                            selectedItemsDictionary[i] = Option(text: "",value: "")
                        }
                    }
                    
                    if parameters.keys.contains("prototype") {
                        
                        controller.newInstance = true
                        controller.newInstanceVersion = parameters["version"]
                        controller.newInstancePrototype = parameters["prototype"]!
                        controller.newInstanceFieldsValues = parameters["fieldsValues"]!
                    
                    } else if (parameters.keys.contains("code")) {
                       
                        controller.instanceCodeFromRequest = parameters["code"]!
                    
                    }
                    
                    controller.selectedItemsDictionary = selectedItemsDictionary;
                }
            } else {
                
                let prototype = searchDictionary["prototype"]! as! String
                let version = searchDictionary["version"]! as! String
                controller.newInstance = true
                controller.newInstancePrototype = prototype
                controller.newInstanceFieldsValues = ""
                controller.selectedItemsDictionary = selectedItemsDictionary;
            
            }
        }
        
    }
    
    func loadFilters() {
        for field in searchDictionary.keys {
            if let index = fieldIndexDictionary[field] {
                if (field == "description" || field == "tagValue") {
                    selectedItemsDictionary[index]?.text = FilterManager.shared[field] ?? ""
                }
                selectedItemsDictionary[index]?.value = FilterManager.shared[field] ?? ""
            }
        }
    }
    
    func saveFilters() {
        for (key, value) in searchDictionary {
            if let stringValue = value as? String {
                FilterManager.shared.setFilter(name: key, value: stringValue)
            }
            if let boolValue = value as? Bool {
                FilterManager.shared.setFilter(name: key, value: boolValue.description)
            }
        }
    }
    
    @IBAction func unwindToSearchInstances(sender: UIStoryboardSegue) {
        
        if let sourceViewController = sender.source as? InstanceViewController {
            selectedItemsDictionary = sourceViewController.selectedItemsDictionary
        }
        
    }
    
    func startActivityIndicator() {
        
        activityIndicator.startAnimating()
        activityIndicator.layer.zPosition = 1
        scrollView.isUserInteractionEnabled = false
    
    }
    
    
    func stopActivityIndicator() {
        
        self.activityIndicator.stopAnimating()
        self.activityIndicator.layer.zPosition = 0
        self.scrollView.isUserInteractionEnabled = true
    
    }
    
}
