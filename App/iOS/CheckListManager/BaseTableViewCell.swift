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
                
        if UIDevice.current.userInterfaceIdiom == .pad {
            self.separatorInset = UIEdgeInsetsMake(0.0, 40.0, 0.0, 40.0)
            self.layoutMargins = UIEdgeInsetsMake(0.0, 0.0, 0.0, 40.0)
        } else {
            self.separatorInset = UIEdgeInsetsMake(0.0, 10.0, 0.0, 10.0)
            self.layoutMargins = UIEdgeInsetsMake(0.0, 0.0, 0.0, 10.0)
        }
    }
}
