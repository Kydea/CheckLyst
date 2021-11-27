//
//  SignerCell.swift
//  Checklyst
//
//  Created by Matematici Developer on 10/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

class SignerCell: BaseTableViewCell {
    
    @IBOutlet var signerCodeLabel: UILabel!
    
    static let identifier = "signerCell"
    
    let accessoryButton: UIButton = {
        let button = UIButton(type: .roundedRect)
        button.titleLabel?.font = UIFont (name: "Flaticon", size: 25)
        button.tintColor = UIColor(red: 0.0/255, green: 150.0/255, blue: 136.0/255, alpha: 1.0)
        button.setTitle("\u{f104}", for: UIControlState())
        button.frame = CGRect(x: 0, y: 0, width: 30, height: 30)
        button.contentMode = .scaleAspectFit
        return button
    }()
    
    required init?(coder aDecoder: NSCoder) {
        
        super.init(coder: aDecoder)
        
        setLayout()
        
        self.accessoryView = accessoryButton as UIView
        
    }

    override init(style: UITableViewCellStyle, reuseIdentifier: String?) {
        
        super.init(style: style, reuseIdentifier: reuseIdentifier)
        
        setLayout()
        
        self.accessoryView = accessoryButton as UIView

    }
    
    func configure(signer: Signer, indexPath: IndexPath) {
        signerCodeLabel.text = signer.code
        accessoryButton.tag = indexPath.row

    }
    
}
