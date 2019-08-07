//
//  UIColor+Conversion.swift
//  Checklyst
//
//  Created by Simone Testa on 28/07/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

extension UIColor {
    
    var hexString:String? {
        if let components = self.cgColor.components {
        let r = components[0]
        let g = components[1]
        let b = components[2]
        return  String(format: "%02X%02X%02X", (Int)(r * 255), (Int)(g * 255), (Int)(b * 255))
        }
        return nil
    }

}
