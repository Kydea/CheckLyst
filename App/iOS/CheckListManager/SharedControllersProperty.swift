//
//  SharedControllersProperty.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 09/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import Foundation

import UIKit
import CoreData



class SharedControllersProperty: NSObject {
    
    var timer : Timer?
    var serverDatetime = Date()
    var optionMenu = UIAlertController(title: nil, message: "Info Menu", preferredStyle: .actionSheet)
    
    static let instance = SharedControllersProperty()

    static func setLogo(_ button: UIBarButtonItem?) -> UIBarButtonItem {
        var button = button
        var logo =  UIImage(named: "logo")
        logo = logo!.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        button = UIBarButtonItem(image: logo, style: UIBarButtonItemStyle.plain, target: nil, action: nil)
        button!.imageInsets = UIEdgeInsetsMake(-5, 30, 10, 0);
        button?.isEnabled=false
        return button!
    }
    
    
    func setInfoButton() -> UIBarButtonItem
    {
        let infoButton = UIButton(type: .infoLight)
        infoButton.tintColor = UIColor(red: 153.0/255, green: 153.0/255, blue: 153.0/255, alpha: 1.0)
        
        infoButton.addTarget(self, action: #selector(getInfoAction), for: .touchUpInside)

        let infoBarButtonItem = UIBarButtonItem(customView: infoButton)
        return infoBarButtonItem
    }
    
    
    @objc func getInfoAction(){
        
        ServerConnection.getAppInfo(){ (result) -> Void in
            do{
                
                if(result.data != nil)
                {
                    let infoResponse = try JSONDecoder().decode(AppInfo.self, from: result.data!)
                    
                    self.serverDatetime = Formatter.instance.stringToDate(infoResponse.serverTime) as Date
                    
                    self.optionMenu = UIAlertController(title: nil, message: "\(Formatter.instance.getInfoTSFromDate(self.serverDatetime))", preferredStyle: .actionSheet)
                    self.optionMenu.modalPresentationStyle = .popover
                    
                    self.timer = Timer.scheduledTimer(timeInterval: 1.0, target: self, selector: #selector(SharedControllersProperty.timerAction), userInfo: nil, repeats: true)
                    
                    let version = infoResponse.version.version ?? "undefined"
                    
                    let serverVersion = UIAlertAction(title: "Server version: \(version)", style: .default, handler: {
                        (alert: UIAlertAction!) -> Void in
                        self.timer?.invalidate()
                    })
                    let versionApp = Bundle.main.infoDictionary!["CFBundleShortVersionString"] as! String!
                    
                    let appVersion = UIAlertAction(title: "App version: \(versionApp ?? "undefined")", style: .default, handler: {
                        (alert: UIAlertAction!) -> Void in
                        self.timer?.invalidate()
                    })
                    
                    let cancelAction = UIAlertAction(title: "Cancel", style: .cancel, handler: {
                        (alert: UIAlertAction!) -> Void in
                        self.timer?.invalidate()
                    })

                    self.optionMenu.addAction(serverVersion)
                    self.optionMenu.addAction(appVersion)
                    self.optionMenu.addAction(cancelAction)
                    
                    
                    if var topController = UIApplication.shared.keyWindow?.rootViewController {
                        
                        while let presentedViewController = topController.presentedViewController {
                            topController = presentedViewController
                        }
                        
                        if topController.isKind(of: UIViewController.self)
                        {
                            self.optionMenu.popoverPresentationController?.permittedArrowDirections = .right
                            self.optionMenu.popoverPresentationController?.sourceView = topController.view
                            self.optionMenu.popoverPresentationController?.sourceRect = CGRect(x:topController.view.bounds.width - 50, y:0, width:0,height:0)
                            topController.present(self.optionMenu, animated: true, completion: nil)
                        }
                    }
                }
            }
            catch{
            }
        }
    }
    
    @objc func timerAction() {
        serverDatetime = serverDatetime.addingTimeInterval(TimeInterval(1))
        optionMenu.message = "\(Formatter.instance.getInfoTSFromDate(self.serverDatetime))"
    }
}
