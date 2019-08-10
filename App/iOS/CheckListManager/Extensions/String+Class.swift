//
//  String+Class.swift
//  Checklyst
//
//  Created by Simone Testa on 28/07/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

extension String {
    
    var classFormat : NSAttributedString? {
        var string = "<span style=\"color:#\(UIColor(red: 0, green: 0, blue: 0, alpha: 1).hexString!); font-size: 14px; font-family: 'Arial', 'Helvetica';\" >\(self)</span>"
        
        string = string.replacingOccurrences(of: "class=\'Error\'", with: "style=\"color:#\(UIColor(red: 255.0/255.0, green: 0, blue: 0, alpha: 1).hexString!); font-size: 14px; font-family: 'Arial', 'Helvetica';\"")
        
        string = string.replacingOccurrences(of: "<span class=\'Null\'></span>", with: "<span style=\"color:#\(UIColor(red: 158.0/255.0, green: 158.0/255.0, blue: 158.0/255.0, alpha: 1).hexString!); font-size: 14px; font-family: 'Arial', 'Helvetica';\">null</span>")
        
        return string.replacingOccurrences(of: "\n", with: "<br />").html2Attributed
    }
}
