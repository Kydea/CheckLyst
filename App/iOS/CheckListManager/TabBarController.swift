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
        
        let label1 = UILabel(frame: CGRect(x: 0, y: 0, width: 30, height: 22))
        
        label1.font =  UIFont (name: "Flaticon", size: 22)
        
        label1.text = "\u{f108}"
        
        let image1 = UIImage.imageWithLabel(label1)
        
        let label2 = UILabel(frame: CGRect(x: 0, y: 0, width: 30, height: 22))
        
        label2.font =  UIFont (name: "Flaticon", size: 22)
        
        label2.text = "\u{f109}"
        
        let image2 = UIImage.imageWithLabel(label2)
        
        let label3 = UILabel(frame: CGRect(x: 0, y: 0, width: 30, height: 22))
        
        label3.font =  UIFont (name: "Flaticon", size: 22)
        
        label3.text = "\u{f106}"
        
        let image3 = UIImage.imageWithLabel(label3)
 
        tabBarItems[0].image = image2
        
        tabBarItems[1].image = image1
        
        tabBarItems[2].image = image3
        
        tabBarItems[0].image = tabBarItems[0].selectedImage?.imageWithColor(UIColor(red: 162.0/255, green: 194.0/255, blue: 205.0/255, alpha: 1.0)).withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[1].image = tabBarItems[1].selectedImage?.imageWithColor(UIColor(red: 162.0/255, green: 194.0/255, blue: 205.0/255, alpha: 1.0)).withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
        tabBarItems[2].image = tabBarItems[2].selectedImage?.imageWithColor(UIColor(red: 162.0/255, green: 194.0/255, blue: 205.0/255, alpha: 1.0)).withRenderingMode(UIImageRenderingMode.alwaysOriginal)
        
    }
    
    func tabBarController(_ tabBarController: UITabBarController, didSelect viewController: UIViewController) {
        if let vc = viewController as? UINavigationController {
            vc.popToRootViewController(animated: false)
        }
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



