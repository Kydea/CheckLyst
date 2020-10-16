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
        
        self.layoutMargins = UIEdgeInsetsMake(0.0, 0.0, 0.0, 40.0)
        
        self.preservesSuperviewLayoutMargins = false
                
        self.separatorInset = UIEdgeInsetsMake(0.0, 40.0, 0.0, 40.0)
    }
}
