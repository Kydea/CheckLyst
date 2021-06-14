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
        
        setLayout()
        
    }
    
    override init(style: UITableViewCellStyle, reuseIdentifier: String?) {
        
        super.init(style: style, reuseIdentifier: reuseIdentifier)
        
        setLayout()
        
    }
    
}
