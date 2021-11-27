//
//  SignersViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 10/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import UIKit

class SignersViewController: UIViewController, UITableViewDataSource, UITableViewDelegate {

    @IBOutlet var tableView: UITableView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        tableView.dataSource = self
        tableView.delegate = self
        let infoBarButtonItem = SharedControllersProperty.instance.setInfoButton()
        self.navigationItem.rightBarButtonItem = infoBarButtonItem
        NotificationCenter.default.addObserver(self, selector: #selector(SignersViewController.signersHaveBeenChanged(_:)), name: .SignersHaveBeenChanged, object: nil)
        setupUI()
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        super.viewWillDisappear(animated)
        if(self.presentedViewController is SignerLoginViewController || self.presentedViewController is SignerPinViewController) {
            self.presentedViewController?.dismiss(animated: false, completion: nil)
        }
    }
    
    func setupUI() {
        self.navigationItem.leftBarButtonItem?.tintColor = UIColor(red: 153.0/255, green: 153.0/255, blue: 153.0/255, alpha: 1.0)
        updateUI()
    }
    
    @objc func signersHaveBeenChanged(_ notification:Notification) {
        updateUI()
    }
    
    private func updateUI() {
        if SignersManager.shared.signers.count == 0 {
            tableView.isHidden = true
        } else {
            tableView.isHidden = false
            tableView.reloadData()
        }
    }
    
    func numberOfSections(in tableView: UITableView) -> Int {
        return 1
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        SignersManager.shared.signers.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        guard let cell = tableView.dequeueReusableCell(withIdentifier: SignerCell.identifier) as? SignerCell else { return UITableViewCell() }
        cell.configure(signer: SignersManager.shared.signers[indexPath.row], indexPath: indexPath)
        cell.accessoryButton.addTarget(self, action: #selector(SignersViewController.accessoryButtonTapped(_:)), for: .touchUpInside)
        cell.setLayout()
        return cell;
    }
    
    @objc func accessoryButtonTapped(_ sender : AnyObject) {
        
        let indexPath = IndexPath(row: sender.tag, section: 0)
        tableView(tableView, didSelectRowAt: indexPath)
    }
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        performSegue(withIdentifier: "signerSetPinSegue", sender: indexPath)
        tableView.deselectRow(at: indexPath, animated: true)
    }
    
    deinit {
        NotificationCenter.default.removeObserver(self)
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        if let controller = segue.destination as? SignerPinViewController {
            guard let index = sender as? IndexPath else { return }
            let signer = SignersManager.shared.signers[index.row]
            controller.signer = signer
        }
    }
    
}
