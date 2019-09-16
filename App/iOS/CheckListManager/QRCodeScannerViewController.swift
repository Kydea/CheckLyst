//
//  QRCodeScannerViewController.swift
//  Checklyst
//
//  Created by Simone Testa on 29/07/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import UIKit
import AVFoundation

class QRCodeScannerViewController: UIViewController, QRScannerViewDelegate {
    

    var callback : ((_ result : String) -> ())?
    var senderController : InstanceViewController?
    
    @IBOutlet var scannerView: QRScannerView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        scannerView.delegate = self
        // Do any additional setup after loading the view.
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        senderController?.customBackButton.isEnabled = false
    }
    
    override func viewDidAppear(_ animated: Bool) {
        super.viewDidAppear(animated)
        AVCaptureDevice.requestAccess(for: .video) { success in
            if success { // if request is granted (success is true)
                if(!self.scannerView.isRunning) {
                        self.scannerView.startScanning()
                    }
            } else { // if request is denied (success is false)
                // Create Alert
                let alert = UIAlertController(title: "Checklyst", message: "Need camera permission for scanning", preferredStyle: .alert)
                
                // Add "OK" Button to alert, pressing it will bring you to the settings app
                alert.addAction(UIAlertAction(title: "Settings", style: .default, handler: { action in
                    self.dismiss(animated: true) {
                        UIApplication.shared.open(URL(string: UIApplicationOpenSettingsURLString)!)
                    }
                }))
                alert.addAction(UIAlertAction(title: "Cancel", style: .destructive, handler: { action in
                        self.dismiss(animated: true, completion: nil)
                }))
                // Show the alert with animation
                DispatchQueue.main.sync {
                    self.present(alert, animated: true)
                }
            }
        }
        
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        super.viewDidDisappear(animated)
        senderController?.customBackButton.isEnabled = true
        if(scannerView.isRunning) {
            scannerView.stopScanning()
        }
    }
    
    func qrScanningDidFail() {
        //print("camera failed")
        let alert = UIAlertController(title: "Error", message: "Scanning Failed. Please try again", preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "Ok", style: .default, handler: nil))
        self.present(alert, animated: false, completion: nil)
    }
    
    func qrScanningSucceededWithCode(_ str: String?) {
        //print("camera succeded " + (str ?? ""))
        if (callback != nil && str != nil) {
            callback!(str!)
            self.dismiss(animated: true, completion: nil)
        }
    }
    
    func qrScanningDidStop() {
        //print("camera stopped")
        self.dismiss(animated: true, completion: nil)
    }

    @IBAction func closeScanner(_ sender: Any) {
        if(scannerView.isRunning) {
            scannerView.stopScanning()
            senderController?.requestForScanning = false
        }
    }
}
