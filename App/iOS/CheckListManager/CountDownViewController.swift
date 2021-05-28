//
//  CountDownViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 25/05/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import UIKit

class CountDownViewController: UIViewController {
    
    private static var instance: CountDownViewController?
    
    static var shared: CountDownViewController {
        if let controller = instance { return controller }
        let storyboard = UIStoryboard(name: "Main", bundle: nil)
        instance = storyboard.instantiateViewController(withIdentifier: "countdown_id") as? CountDownViewController
        return instance!
    }

    @IBOutlet var countdownLabel: UILabel?
    
    var completition: (() -> Void)?
    
    var isPresented: Bool = false
    
    override func viewDidLoad() {
        super.viewDidLoad()
        countdownLabel?.text = ""
    }
    
    override func viewWillAppear(_ animated: Bool) {
        isPresented = true
    }
    
    override func viewDidDisappear(_ animated: Bool) {
        isPresented = false
    }
        
    func updateCountDown(time: TimeInterval) {
        print("countdown time: \(time)")
        countdownLabel?.text = time > 0 ? Int(time).description : "0"
        
        if (time <= 0) {
            self.dismiss(animated: false, completion: completition)
        }
    }
    
    func reset() {
        completition = nil
        countdownLabel?.text = ""
        self.dismiss(animated: false, completion: nil)
    }
}
