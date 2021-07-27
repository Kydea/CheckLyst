//
//  LoadingViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 27/07/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import UIKit

class LoadingViewController: UIViewController {

    private static var instance: LoadingViewController?
    
    @IBOutlet var activityIndicator: UIActivityIndicatorView!
        
    static var shared: LoadingViewController {
        if let controller = instance { return controller }
        let storyboard = UIStoryboard(name: "Main", bundle: nil)
        instance = storyboard.instantiateViewController(withIdentifier: "loading_id") as? LoadingViewController
        instance?.setupStyle()
        return instance!
    }
        
    var isPresented: Bool = false
    
    override func viewDidLoad() {
        super.viewDidLoad()
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)

    }
    
    override func viewWillAppear(_ animated: Bool) {
        isPresented = true
    }
    
    override func viewDidDisappear(_ animated: Bool) {
        isPresented = false
    }
    
    private func setupStyle() {
        self.modalPresentationStyle = .overCurrentContext
        //self.modalTransitionStyle = .crossDissolve
        self.view.backgroundColor = UIColor.clear
        self.view.isOpaque = false
    }
    
    func startLoading(controller: UIViewController) {
        if (!isPresented) {
            #if DEBUG
            print("Loading started")
            #endif
            controller.present(self, animated: false, completion: nil)
            activityIndicator.startAnimating()
            isPresented = true
        }
    }
    
    func stopLoading() {
        if (isPresented) {
            #if DEBUG
            print("Loading stopped")
            #endif
            self.dismiss(animated: false, completion: nil)
            activityIndicator.stopAnimating()
            isPresented = false
        }
    }
}
