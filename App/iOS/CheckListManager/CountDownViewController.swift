//
//  CountDownViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 25/05/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import UIKit

class CountDownViewController: UIViewController {

    @IBOutlet var countdownLabel: UILabel!
    
    override func viewDidLoad() {
        super.viewDidLoad()

    }
    
    func updateCountDown(time: TimeInterval) {
        countdownLabel.text = time > 0 ? Int(time).description : "0"
        
        if (time <= 0) {
            self.dismiss(animated: true, completion: nil)
        }
    }
}
