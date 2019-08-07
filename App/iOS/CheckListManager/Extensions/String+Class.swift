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
    var errorFormat : NSAttributedString? {
        
        return self.htmlAttributed(family: "Arial", size: 14.0, color: UIColor(red: 255.0/255.0, green: 0, blue: 0, alpha: 1), keepReturn: true)
    }
    
    var defaultFormat : NSAttributedString? {
        return self.htmlAttributed(family: "Arial", size: 14.0, color: UIColor(red: 0, green: 0, blue: 0, alpha: 1), keepReturn: true)
    }
    
    var nullFormat : NSAttributedString? {
        return self.htmlAttributed(family: "Arial", size: 14.0, color: UIColor(red: 158.0/255.0, green: 158.0/255.0, blue: 158.0/255.0, alpha: 1), keepReturn: true)
    }
    
    func replacingOccurrences(regex pattern : String, with: String) -> String {
        do {
            let regex = try NSRegularExpression(pattern: pattern, options: NSRegularExpression.Options.caseInsensitive)
            let range = NSMakeRange(0, self.count)
            return regex.stringByReplacingMatches(in: self, options: [], range: range, withTemplate: with)
        } catch {
            return self
        }
    }
    
    var classFormat : NSAttributedString? {
        var string = "<span style=\"color:#\(UIColor(red: 0, green: 0, blue: 0, alpha: 1).hexString!); font-size: 14px !important; font-family: 'Arial', 'Helvetica' !important;\" >\(self)</span>"
        string = string.replacingOccurrences(of: "class=\'Error\'", with: "style=\"color:#\(UIColor(red: 255.0/255.0, green: 0, blue: 0, alpha: 1).hexString!); font-size: 14px !important; font-family: 'Arial', 'Helvetica' !important;\"")
        string = string.replacingOccurrences(of: "<span class=\'Null\'></span>", with: "<span style=\"color:#\(UIColor(red: 158.0/255.0, green: 158.0/255.0, blue: 158.0/255.0, alpha: 1).hexString!); font-size: 14px !important; font-family: 'Arial', 'Helvetica' !important;\">null</span>")
        return string.replacingOccurrences(of: "\n", with: "<br />").html2Attributed
    }
}
