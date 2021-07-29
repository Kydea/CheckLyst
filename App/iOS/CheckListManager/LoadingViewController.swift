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
        
    var isPresenting: Bool = false
    var isPresented: Bool = false

    
    override func viewDidLoad() {
        super.viewDidLoad()
        activityIndicator.transform = CGAffineTransform(scaleX: 2.0, y: 2.0)

    }
    
    private func setupStyle() {
        self.modalPresentationStyle = .overCurrentContext
        //self.modalTransitionStyle = .crossDissolve
        self.view.backgroundColor = UIColor.clear
        self.view.isOpaque = false
    }
    
    func startLoading(controller: UIViewController, completition: (() -> Void)? = nil) {
        if (!isPresenting && !isPresented) {
            #if DEBUG
            print("Loading started")
            #endif
            isPresenting = true
            controller.present(self, animated: false) { [weak self] in
                self?.activityIndicator.startAnimating()
                self?.isPresented = true
                self?.isPresenting = false
                completition?()
            }
            
        }
    }
    
    func stopLoading(controller: UIViewController?, completition: (() -> Void)? = nil) {
        if (isPresented) {
            #if DEBUG
            print("Loading stopped")
            #endif
            controller?.dismiss(animated: false) { [weak self] in
                self?.activityIndicator.stopAnimating()
                self?.isPresented = false
                completition?()
            }
        }
    }
}
