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
    
    override func sizeThatFits(_ size: CGSize) -> CGSize {
        
        var sizeThatFits = super.sizeThatFits(size)
        
        sizeThatFits.height = 70
        
        return sizeThatFits
    }
}

