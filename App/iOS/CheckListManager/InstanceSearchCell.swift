//
//  instanceSearchCell.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 08/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

class InstanceSearchCell: UITableViewCell {
    
    required init?(coder aDecoder: NSCoder) {
        
        super.init(coder: aDecoder)
        
        self.layoutMargins = UIEdgeInsetsMake(0.0, 0.0, 0.0, 40.0)
        
        self.preservesSuperviewLayoutMargins = false
        
        self.separatorInset = UIEdgeInsetsMake(0.0, 40.0, 0.0, 40.0)
        
    }
}
