//
//  ListInstancesCell.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 07/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation

import UIKit

class ListInstancesCell: BaseTableViewCell {
    
    @IBOutlet var stateLabelTopConstraint: NSLayoutConstraint!
    
    @IBOutlet var descrizione: UILabel!

   
    required init?(coder aDecoder: NSCoder) {
        
        super.init(coder: aDecoder)
        
        self.layoutMargins = UIEdgeInsetsMake(0.0, 0.0, 0.0, 40.0)
                
        self.preservesSuperviewLayoutMargins = false
        
        self.separatorInset = UIEdgeInsetsMake(0.0, 40.0, 0.0, 40.0)
        
    }
    
    override init(style: UITableViewCellStyle, reuseIdentifier: String?) {
        
        super.init(style: style, reuseIdentifier: reuseIdentifier)
        
        self.layoutMargins = UIEdgeInsetsMake(0.0, 0.0, 0.0, 40.0)
        
        self.preservesSuperviewLayoutMargins = false
                
        self.separatorInset = UIEdgeInsetsMake(0.0, 40.0, 0.0, 40.0)
    }
    
}
