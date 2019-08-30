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
        
        var string = self.replacingOccurrences(of: "<span class=\'Null\'></span>", with: "")
        
        if (string.replacingOccurrences(of: "\n", with: "").count == 0) {
            return nil
        }
        
        string = "<span style=\"color:#\(UIColor(red: 0, green: 0, blue: 0, alpha: 1).hexString!); font-size: 14px; font-family: 'Arial', 'Helvetica';\" >\(string)</span>"
        
        string = string.replacingOccurrences(of: "class=\'Error\'", with: "style=\"color:#\(UIColor(red: 255.0/255.0, green: 0, blue: 0, alpha: 1).hexString!); font-size: 14px; font-family: 'Arial', 'Helvetica';\"")
        
        return string.replacingOccurrences(of: "\n", with: "<br />").html2Attributed
    }
}
