//
//  Swift+Extension.swift
//  Checklyst
//
//  Created by Simone Testa on 27/07/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

extension String {
    
    func getStringHeight(font : UIFont) -> CGFloat {
            let value = NSAttributedString(string: self, attributes: [NSAttributedStringKey.font: font])
            return ceil(value.size().height)
    }
 
    func getHTMLStringHeight(font : UIFont) -> CGFloat {
        if let value = htmlAttributed(using: font, color: UIColor(red: 0, green: 0, blue: 0, alpha: 1), keepReturn: true) {
            return ceil(value.size().height)
        } else {
            return 0.0
        }
    }
    
}
