//
//  TabBarController.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 23/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import UIKit
import Foundation

class TabViewController: UITabBarController, UITabBarControllerDelegate {
    
    
    var userCode: String?
    
    override func viewDidLoad() {
 
        super.viewDidLoad()
        
        self.delegate = self;
        
    }
    
    override func viewDidAppear(_ animated: Bool) {
        
        super.viewDidAppear(animated)
        
        let tabBarItems = tabBar.items! as [UITabBarItem]
        
        tabBarItems[0].image = UIImage(named: "userIcon")?.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[1].image = UIImage(named: "searchIcon")?.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[2].image = UIImage(named: "helpIcon")?.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[3].image = UIImage(named: "signatureIcon")?.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[0].selectedImage = UIImage(named: "userSelectedIcon")?.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[1].selectedImage = UIImage(named: "searchSelectedIcon")?.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[2].selectedImage = UIImage(named: "helpSelectedIcon")?.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[3].selectedImage = UIImage(named: "signatureSelectedIcon")?.withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
    }
    
    func tabBarController(_ tabBarController: UITabBarController, shouldSelect viewController: UIViewController) -> Bool {
        return viewController != tabBarController.selectedViewController
    }
    
}




extension UIImage {
    
    class func imageWithLabel(_ label: UILabel) -> UIImage {
        
        UIGraphicsBeginImageContextWithOptions(label.bounds.size, false, 0.0)
        
        label.layer.render(in: UIGraphicsGetCurrentContext()!)
        
        let img = UIGraphicsGetImageFromCurrentImageContext()
        
        UIGraphicsEndImageContext()
        
        return img!
    }
}


extension UIImage {
    
    func imageWithColor(_ color1: UIColor) -> UIImage {
        
        UIGraphicsBeginImageContextWithOptions(self.size, false, self.scale)
        
        color1.setFill()
        
        let context = UIGraphicsGetCurrentContext()
        
        context!.translateBy(x: 0, y: self.size.height)
        
        context!.scaleBy(x: 1.0, y: -1.0);
        
        context!.setBlendMode(CGBlendMode.normal)
        
        let rect = CGRect(x: 0, y: 0, width: self.size.width, height: self.size.height) as CGRect
        
        context!.clip(to: rect, mask: self.cgImage!)
        
        context!.fill(rect)
        
        let newImage = UIGraphicsGetImageFromCurrentImageContext()! as UIImage
        
        UIGraphicsEndImageContext()
        
        return newImage
    }
}



