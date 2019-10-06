//
//  TabBar.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 05/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation

import UIKit



class TabBar: UITabBar {
    
    /*
     *  Currently resize is applied only on versions before iOS13.
     *  Changing the tabbar's height produces issues in icon alignment (in this case just text,
     *  because we use custom image from labels)
     */
    
    override func sizeThatFits(_ size: CGSize) -> CGSize {
        
        var sizeThatFits = super.sizeThatFits(size)
       
        /*if #available(iOS 13.0, *) {
            
            return sizeThatFits
        
        }
        
        sizeThatFits.height = 70*/
        
        return sizeThatFits
    }
}

