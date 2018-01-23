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
    
    /*  static text for the labels */
    var indexTextDictionary =
    [0:"Departments", 1:"Prototype Types", 2:"Prototype", 3:"Version", 4:"Description", 5:"Tags", 6:"Tags Value" ,7:"States"]
    
    /*  values sent to the server like query    */
    var searchDictionary : Dictionary<String, String?> = ["department" : nil, "description" : "", "prototype" : nil, "prototypeType": nil, "state" : nil, "tagCode" : nil, "tagValue" : "", "version" : nil]
    
    var fieldIndexDictionary =
    ["department":0, "prototypeType":1, "prototype":2, "version":3, "description":4, "tagCode":5, "tagValue":6 ,"state":7]
    
    var indexFieldDictionary =
    [0:"department", 1:"prototypeType", 2:"prototype", 3:"version", 4:"description", 5:"tagCode", 6:"tagValue", 7:"state"]
    
    var selectedItemsDictionary = [Int:Option] ()
    
    var indexArrayDictionary = [Int:[Option?]] ()
    
    var statesArray: [String] = []
    
    var numberOfPages = 0
    
    
    override func viewDidLoad()
    {
        super.viewDidLoad()
        
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        navigationItem.setRightBarButtonItems([infoBarButtonItem,navigationItem.rightBarButtonItem!], animated: false)

        self.tableView.tableFooterView =  UIView()
        self.tableView.isScrollEnabled = false
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)
        activityIndicator.hidesWhenStopped = true
        newInstanceButton.isEnabled = false
        newInstanceButton.title = "\u{f103} New Instance   "
        
        for i in 0...7 {
            selectedItemsDictionary[i] = Option(text: "",value: "")
        }
    }
    
    
    override func viewDidAppear(_ animated: Bool)
    {
        self.navigationItem.leftBarButtonItem = SharedControllersProperty.setLogo(self.navigationItem.leftBarButtonItem);
        fromSelectionToSearchDictionary()
        fillSearchFields()
    }
    
    
    
    
    func fromSelectionToSearchDictionary()
    {
        
        for field in searchDictionary.keys {
            let index = fieldIndexDictionary[field]!
            searchDictionary[field] = (field == "version") ? selectedItemsDictionary[index]!.value.components(separatedBy: " -")[0] : searchDictionary[field]
            searchDictionary[field] = (searchDictionary[field] == nil) ? "" : selectedItemsDictionary[fieldIndexDictionary[field]!]!.value
        }
    }
    
    
    func numberOfSections(in tableView: UITableView) -> Int {
        return 1
    }
    
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return 8
    }
    
    
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell
    {
        
        let cell = tableView.dequeueReusableCell(withIdentifier: "searchCell")
        cell!.preservesSuperviewLayoutMargins = false
        let accessoryButton = UIButton(type: .roundedRect)
        accessoryButton.tag = indexPath.row
        accessoryButton.titleLabel?.font = UIFont (name: "Flaticon", size: 25)
        accessoryButton.tintColor = UIColor(red: 82.0/255, green: 140.0/255, blue: 162.0/255, alpha: 1.0)
        accessoryButton.setTitle("\u{f104}", for: UIControlState())
        accessoryButton.frame = CGRect(x: 0, y: 0, width: 30, height: 30)
        accessoryButton.contentMode = .scaleAspectFit
        cell!.accessoryView = accessoryButton as UIView
        let fieldLabel = cell!.contentView.viewWithTag(1) as? UILabel
        fieldLabel?.text = indexTextDictionary[indexPath.row]
        let fieldValue = cell!.contentView.viewWithTag(2) as? UILabel
        fieldValue?.text = selectedItemsDictionary[indexPath.row]?.text
        cell?.isUserInteractionEnabled = true
        fieldValue!.isEnabled = true;
        
        let isList = !(indexTextDictionary[indexPath.row] == "Description" || indexTextDictionary[indexPath.row] == "Tags Value") ? true : false

        fieldValue?.textColor = (isList) ? UIColor.black : fieldValue?.textColor
        
        var blocked: Bool = indexArrayDictionary[indexPath.row] != nil && indexArrayDictionary[indexPath.row]!.count <= 1 ? true : false

        
        if(selectedItemsDictionary[fieldIndexDictionary["version"]!]?.text == "" && indexPath.row == fieldIndexDictionary["tagCode"]!)
        {
            blocked = true
            selectedItemsDictionary[fieldIndexDictionary["tagCode"]!]?.text = ""
            fieldValue?.text = ""
        }
        
        if(indexPath.row == fieldIndexDictionary["description"]) {
            if(selectedItemsDictionary[fieldIndexDictionary["description"]!]?.text == "") {
                selectedItemsDictionary[fieldIndexDictionary["description"]!]?.text = "Insert"
            }
        }
  
        if(indexPath.row == fieldIndexDictionary["tagValue"])
        {
            if((selectedItemsDictionary[fieldIndexDictionary["tagCode"]!]?.text == "All" || selectedItemsDictionary[fieldIndexDictionary["tagCode"]!]?.text == ""))
            {
                selectedItemsDictionary[fieldIndexDictionary["tagValue"]!]?.text = ""
                fieldValue?.text = ""
                blocked = true
            }
                
            else if(selectedItemsDictionary[fieldIndexDictionary["tagValue"]!]?.text == "")
            {
                blocked = false
                selectedItemsDictionary[fieldIndexDictionary["tagValue"]!]?.text = "Insert"
                fieldValue?.text = "Insert"
            }
        }
        
        if(blocked){
            cell?.isUserInteractionEnabled = false
            fieldValue!.isEnabled = false;
            accessoryButton.setTitle("\u{f100}", for: UIControlState())
        }
        
        if(!isList){
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
    
    
    func fillSearchFields()
    {
        self.startActivityIndicator()
        
        ServerConnection.bindListWithParams(["data" : searchDictionary]) { (serverResponse) -> Void in
            
            do
            {
                if(serverResponse?.data != nil)
                {
                    
                    let searchFields = try JSONDecoder().decode(IstanceBindResponseJSON.self, from: serverResponse!.data!)

                    if (searchFields.fields.department != nil) {
                        guard let departments = searchFields.fields.department?.options else {
                            print("No departments")
                            self.stopActivityIndicator()
                            return
                        }
                    
                        if departments.count == 0 {
                            self.displayMessage("Error", messageText: "The user does not have instance grants on any department of this company")
                        }
                    
                        self.indexArrayDictionary[self.fieldIndexDictionary["department"]!] = departments
                        self.indexArrayDictionary[self.fieldIndexDictionary["department"]!]?.insert(Option(text: "All", value: ""), at: 0)
                    }
                    
                    if(searchFields.fields.prototypeType != nil) {
                        guard let prototypeTypes = searchFields.fields.prototypeType?.options else {
                            print("No prototypeTypes")
                            self.stopActivityIndicator()
                            return
                        }
                        
                        if prototypeTypes.count == 0 {
                            //self.displayMessage("Error", messageText: "The user does not have instance grants on any prototype type of this department")
                        }
                        
                        self.indexArrayDictionary[self.fieldIndexDictionary["prototypeType"]!] = prototypeTypes
                        self.indexArrayDictionary[self.fieldIndexDictionary["prototypeType"]!]?.insert(Option(text: "All", value: ""), at: 0)
                    }
                    
                    if(searchFields.fields.prototype != nil) {
                        guard let prototype = searchFields.fields.prototype?.options else {
                            print("No prototype")
                            self.stopActivityIndicator()
                            return
                        }
                        
                        if prototype.count == 0 {
                            //self.displayMessage("Error", messageText: "The user does not have instance grants on any prototype of this type")
                        }
                        
                        self.indexArrayDictionary[self.fieldIndexDictionary["prototype"]!] = prototype
                        self.indexArrayDictionary[self.fieldIndexDictionary["prototype"]!]?.insert(Option(text: "All", value: ""), at: 0)
                    }
                    
                    if(searchFields.fields.version != nil) {
                    
                        guard let versions = searchFields.fields.version?.options else {
                            print("No versions")
                            self.stopActivityIndicator()
                            return
                        }
                        
                        self.indexArrayDictionary[self.fieldIndexDictionary["version"]!] = versions
                        self.indexArrayDictionary[self.fieldIndexDictionary["version"]!]?.insert(Option(text: "All", value: ""), at: 0)
                    }
                    
                    if(searchFields.fields.tagCode != nil) {
                    
                        guard let tags = searchFields.fields.tagCode?.options else {
                            print("No tags")
                            self.stopActivityIndicator()
                            return
                        }
                    
                        self.indexArrayDictionary[self.fieldIndexDictionary["tagCode"]!] = tags
                        self.indexArrayDictionary[self.fieldIndexDictionary["tagCode"]!]?.insert(Option(text: "All", value: ""), at: 0)
                    }
                    
                    if(searchFields.fields.state != nil) {
                        
                        guard let states = searchFields.fields.state?.options else {
                            print("No state")
                            self.stopActivityIndicator()
                            return
                        }
                        
                        self.indexArrayDictionary[self.fieldIndexDictionary["state"]!] = states
                        self.indexArrayDictionary[self.fieldIndexDictionary["state"]!]?.insert(Option(text: "All", value: ""), at: 0)
                    }
                    
                    self.setLabelTitles()
                }
                
                else
                {
                    let alert = UIAlertController(title: "Error", message: "Connection error to the server", preferredStyle: .alert)
                    
                    alert.addAction(UIAlertAction(title: "Ok", style: .default, handler: { (action) -> Void in
    
                        let app = UIApplication.shared as! MyApplication
                        
                        app.logoutAndGoToLogin()
                    }))
                    
                    self.present(alert, animated: false, completion: nil)
                }
            }
            catch {
                print(error)
                self.stopActivityIndicator()
            }
            self.stopActivityIndicator()
        }
    }
    
    func findOPtionInIndexArrayDictionaryForValue(_ value:String, inField field:String, andIndex index:Int) -> Option?
    {
        for element in indexArrayDictionary[index]! {
            if(element!.value == value) {
                return element
            }
        }
        return nil
    }

    
    func setLabelTitles()
    {
        for index in indexArrayDictionary.keys{
            
            let field = indexFieldDictionary[index]!
            
            if let option = findOPtionInIndexArrayDictionaryForValue(self.searchDictionary[field]!!, inField: field, andIndex: index)
            {
                self.selectedItemsDictionary[index]?.text = option.text
                self.selectedItemsDictionary[index]?.value = option.value
            }
                
            else
            {
                print(self.selectedItemsDictionary[index]!)
                print(indexArrayDictionary[index]!)
                if(indexArrayDictionary[index]!.count > 0) {
                    self.selectedItemsDictionary[index] = indexArrayDictionary[index]![0]
                } else {
                    self.selectedItemsDictionary[index] = Option(text: "", value: "")
                }
            }
        }
        self.newInstanceButton.isEnabled = self.selectedItemsDictionary[fieldIndexDictionary["version"]!]!.text.contains("Active") ? true : false
        
        self.tableView.reloadData()
    }
    

    func displayMessage(_ messageTitle:String, messageText:String)
        
    {
        let alert = UIAlertController(title: messageTitle, message: messageText, preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "Ok", style: .destructive, handler: nil))
        self.present(alert, animated: false, completion: nil)
    }
    

    
    @IBAction func searchInstances(_ sender: AnyObject)
        
    {
        
        self.startActivityIndicator()
        self.listInstances = []
        self.getResearchResultForPage(currentPage: 1)
        
    }
    
    func getResearchResultForPage(currentPage:Int) {
        var connectionError = false
        
        ServerConnection.getListInstancesWithParams(["CurrentPage" : String(currentPage), "data" : searchDictionary]) { (serverResponse) -> Void in
            
            do
            {
                
                if(serverResponse?.data != nil)
                    
                {
                    
                    let listIstancesJSON = try JSONDecoder().decode(ListIstancesJSON.self, from: serverResponse!.data!)

                    self.listInstances += listIstancesJSON.data
                    
                    self.numberOfPages = listIstancesJSON.pagesCount
                    
                }
                    
                else
                {
                    self.displayMessage("Error", messageText: "Connection error to the server")
                    
                    connectionError = true
                }
            }
                
            catch {
                
                print(error)
                
                do {
                    
                    let json = try JSONSerialization.jsonObject(with: serverResponse!.data!, options: .allowFragments) as? [String:[Any]]
                    self.displayMessage("Error", messageText: json!["messages"]![0] as! String)
                    
                }
                catch {
                    
                    print(error)
                    
                }
            }
            
            if(self.listInstances.count == 0 && !connectionError)
                
            {
                self.stopActivityIndicator()
                self.displayMessage("Error", messageText: "No records found")
            }
                
                
            else if(!connectionError && currentPage == self.numberOfPages)
            {
                self.stopActivityIndicator()
                self.performSegue(withIdentifier: "instanceDetailsSegue", sender: self)
            } else {
                self.getResearchResultForPage(currentPage: currentPage + 1)
            }
        }
    }
    

    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        
        if let controller = segue.destination as? EditFieldViewController
        {
            let indexPath = sender as! IndexPath
            controller.fieldsArray = indexArrayDictionary[indexPath.row] as? [Option]
            controller.searchViewController = self
            controller.rowIndex = indexPath.row
            controller.isListField = (indexTextDictionary[indexPath.row] == "Description" || indexTextDictionary[indexPath.row] == "Tags Value") ? false : true
            controller.fieldNameString = indexTextDictionary[indexPath.row]
            controller.indexFieldDictionary = indexFieldDictionary
        }
            
        else if let controller = segue.destination as? ListInstancesTableViewController {
            controller.instancesList = self.listInstances as! [Instance]
        }
        
        else if let controller = segue.destination as? InstanceViewController {
            controller.newInstance = true
            controller.newInstancePrototype = searchDictionary["prototype"]!
            controller.newInstanceVersion = searchDictionary["version"]!
        }
        
    }
    
    func startActivityIndicator(){
        activityIndicator.startAnimating()
        activityIndicator.layer.zPosition = 1
        scrollView.isUserInteractionEnabled = false
    }
    
    
    func stopActivityIndicator()
    {
        self.activityIndicator.stopAnimating()
        self.activityIndicator.layer.zPosition = 0
        self.scrollView.isUserInteractionEnabled = true
    }
    
}
