//
//  BaseTableViewCell.swift
//  Checklyst
//
//  Created by Matematici Developer on 16/10/2020.
//  Copyright © 2020 Johnson&Johnson. All rights reserved.
//

import UIKit

class BaseTableViewCell: UITableViewCell {

    func setLayout() {
        
        
        self.preservesSuperviewLayoutMargins = false
                
        if shouldUseSlimLayout() {
            self.separatorInset = UIEdgeInsetsMake(0.0, 10.0, 0.0, 10.0)
            self.layoutMargins = UIEdgeInsetsMake(0.0, 0.0, 0.0, 10.0)
        } else {
            self.separatorInset = UIEdgeInsetsMake(0.0, 40.0, 0.0, 40.0)
            self.layoutMargins = UIEdgeInsetsMake(0.0, 0.0, 0.0, 40.0)
        }
    }
    
    private func shouldUseSlimLayout() -> Bool {
        let currentDevice = UIDevice.current
        return currentDevice.userInterfaceIdiom == .phone &&
            (currentDevice.orientation == .portrait || currentDevice.orientation == .portraitUpsideDown)
        
    }
}
