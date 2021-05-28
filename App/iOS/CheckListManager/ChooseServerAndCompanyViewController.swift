//
//  ChooseServerViewController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 24/02/17.
//  Copyright © 2017 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

class ChooseServerAndCompanyViewController: UIViewController, UITableViewDelegate, UITableViewDataSource, UISearchResultsUpdating {
    
    @IBOutlet var tableView: UITableView!
    
    @IBOutlet var okButton: UIButton!
    
    var companyFieldsDictionary: [CompanyShortJSON]?
    
    var filteredCompanyFields: [CompanyShortJSON] {
        guard let text = searchController.searchBar.text, !text.isEmpty else {
            return companyFieldsDictionary ?? []
        }
        return companyFieldsDictionary?.filter({$0.name.lowercased().contains(text.lowercased())}) ?? []
    }
    
    var serverFieldsDictionary: [Server]?
    
    var filteredServerFields: [Server] {
        guard let text = searchController.searchBar.text, !text.isEmpty else {
            return serverFieldsDictionary ?? []
        }
        return serverFieldsDictionary?.filter({ ($0.serverFrontEndUrl?.lowercased().contains(text.lowercased()) ?? false) }) ?? []
    }
    
    var rowIndex : Int?
    
    var loginViewController: LoginViewController?
    
    var secondLine: UIView!
    
    @IBOutlet var innerView: UIView!
    
    var fieldNameString: String?
    
    var inputTextField: UITextField?
    
    var chooseServer : Bool!
    
    @IBOutlet var fieldNameLabel: UILabel!
    
    let context = ((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)
    
    let searchController = UISearchController(searchResultsController: nil)
        
    override func viewDidLoad() {
        
        fieldNameLabel?.text = fieldNameString
        
        okButton.isHidden = true
        
        searchController.searchResultsUpdater = self
        searchController.searchBar.autocapitalizationType = .none
        searchController.searchBar.tintColor = .white
        self.navigationItem.hidesSearchBarWhenScrolling = false
        self.navigationItem.searchController = searchController
        definesPresentationContext = true
        searchController.obscuresBackgroundDuringPresentation = false
    }
    
    func numberOfSections(in tableView: UITableView) -> Int {
        
        return 1
        
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        
        return (chooseServer ? filteredServerFields.count : filteredCompanyFields.count)
    
    }
    
    
    
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        
        let cell = tableView.dequeueReusableCell(withIdentifier: "searchCell")
        
        let fieldLabel = cell!.contentView.viewWithTag(1) as? UILabel
        
        inputTextField = cell!.contentView.viewWithTag(2) as? UITextField
        
        
        fieldLabel?.text = chooseServer ? filteredServerFields[indexPath.row].serverFrontEndUrl : filteredCompanyFields[indexPath.row].name
        inputTextField?.isHidden = true
        
        
        cell!.preservesSuperviewLayoutMargins = false
        
        return cell!
    }
    
    
    
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        
        if(chooseServer!) {
            
            let server = filteredServerFields[indexPath.row]
            
            loginViewController?.serverTextField.text = server.serverFrontEndUrl
            
            loginViewController?.textFieldDidEndEditing(loginViewController!.serverTextField)
            
        } else {
            print(filteredCompanyFields.debugDescription)
            print(indexPath.row)
            let company = filteredCompanyFields[indexPath.row]
            loginViewController?.companyTextField.text = company.name
            
            loginViewController?.selectedCompany = company
        }
        
        self.navigationController?.popViewController(animated: true)
        
    }
    
    func updateSearchResults(for searchController: UISearchController) {
        print(searchController.searchBar.text ?? "")
        tableView.reloadData()
    }
    
    override func viewWillDisappear(_ animated : Bool) {
        
        super.viewWillDisappear(animated)
        
    }
    
    
    
    @IBAction func saveEditing(_ sender: AnyObject) {
        
        self.navigationController?.popViewController(animated: true)
    }
    
    
}
